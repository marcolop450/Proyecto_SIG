using Mapsui;
using Mapsui.Projections;
using Mapsui.Tiling;

namespace VisorDatosSIG.Mobile.Views;

public partial class MapPage : ContentPage
{
    public MapPage()
    {
        InitializeComponent();
        InitializeMap();
    }

    private void InitializeMap()
    {
        var map = new Mapsui.Map
        {
            CRS = "EPSG:3857"
        };

        // Agregar OpenStreetMap con User-Agent personalizado para evitar bloqueo
        map.Layers.Add(OpenStreetMap.CreateTileLayer("VisorDatosSIG.Mobile/1.0"));

        // Centrar en Santa Cruz de la Sierra (aprox)
        var (x, y) = SphericalMercator.FromLonLat(-63.18, -17.78);
        var center = new MPoint(x, y);
        
        mapView.Map = map;

        // Necesitamos esperar a que el control se cargue para navegar
        mapView.Loaded += (s, e) => 
        {
            mapView.Map.Navigator.CenterOn(center);
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        string token = Preferences.Default.Get("jwt_token", string.Empty);
        if (string.IsNullOrEmpty(token)) return;

        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        var apiService = new VisorDatosSIG.Mobile.Services.ApiService();
        var geojson = await apiService.GetManzanasGeoJsonAsync(token);

        if (!string.IsNullOrEmpty(geojson))
        {
            try
            {
                var reader = new NetTopologySuite.IO.GeoJsonReader();
                var featureCollection = reader.Read<NetTopologySuite.Features.FeatureCollection>(geojson);
                
                var features = new List<Mapsui.Nts.GeometryFeature>();
                foreach (var f in featureCollection)
                {
                    // Convertir de WGS84 (Lat/Lon) a EPSG:3857 (Spherical Mercator) para el mapa web
                    // (Asumiendo que el GeoJSON viene en Lat/Lon estándar)
                    var geom = f.Geometry;
                    // Se debería proyectar la geometría, pero para prueba rápida Mapsui tiene un MemoryLayer proyectado o lo proyectamos
                    features.Add(new Mapsui.Nts.GeometryFeature(geom));
                }

                var provider = new Mapsui.Providers.MemoryProvider(features) { CRS = "EPSG:4326" };
                
                // Mapsui proyecta automáticamente si configuramos el CRS
                var layer = new Mapsui.Layers.Layer("Manzanas") 
                { 
                    DataSource = provider,
                    Style = new Mapsui.Styles.VectorStyle 
                    { 
                        Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.FromArgb(128, 0, 120, 255)), // Azul semitransparente
                        Outline = new Mapsui.Styles.Pen(Mapsui.Styles.Color.Blue, 2)
                    }
                };

                mapView.Map.Layers.Add(layer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parseando GeoJSON: {ex.Message}");
            }
        }

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        var query = SearchEntry.Text;
        if (string.IsNullOrWhiteSpace(query)) return;

        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        try
        {
            // Búsqueda general usando Nominatim de OpenStreetMap
            using var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "VisorDatosSIG.Mobile");
            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=1";
            var response = await client.GetStringAsync(url);

            var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(response);
            if (result.ValueKind == System.Text.Json.JsonValueKind.Array && result.GetArrayLength() > 0)
            {
                var first = result[0];
                if (first.TryGetProperty("lat", out var latProp) && first.TryGetProperty("lon", out var lonProp))
                {
                    double lat = double.Parse(latProp.GetString() ?? "0", System.Globalization.CultureInfo.InvariantCulture);
                    double lon = double.Parse(lonProp.GetString() ?? "0", System.Globalization.CultureInfo.InvariantCulture);

                    var (x, y) = Mapsui.Projections.SphericalMercator.FromLonLat(lon, lat);
                    var point = new Mapsui.MPoint(x, y);

                    // Centrar mapa
                    mapView.Map.Navigator.CenterOn(point);
                    mapView.Map.Navigator.ZoomToLevel(12);

                    // Agregar Marcador (Pin)
                    var pinFeature = new Mapsui.Nts.GeometryFeature { Geometry = new NetTopologySuite.Geometries.Point(lon, lat) };
                    var pinProvider = new Mapsui.Providers.MemoryProvider(pinFeature) { CRS = "EPSG:4326" };
                    var pinLayer = new Mapsui.Layers.Layer("Resultado Búsqueda")
                    {
                        DataSource = pinProvider,
                        Style = new Mapsui.Styles.SymbolStyle
                        {
                            Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Red),
                            SymbolScale = 0.5,
                            Outline = new Mapsui.Styles.Pen(Mapsui.Styles.Color.White, 2)
                        }
                    };
                    
                    // Remover marcadores anteriores
                    var oldLayers = mapView.Map.Layers.Where(l => l.Name == "Resultado Búsqueda").ToList();
                    foreach(var l in oldLayers) mapView.Map.Layers.Remove(l);

                    mapView.Map.Layers.Add(pinLayer);
                }
            }
            else
            {
                await DisplayAlert("Sin resultados", "No se encontró el lugar buscado.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Ocurrió un error al buscar: " + ex.Message, "OK");
        }

        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;
    }

    private void OnHomeClicked(object sender, EventArgs e)
    {
        // Volver a Santa Cruz de la Sierra
        var (x, y) = Mapsui.Projections.SphericalMercator.FromLonLat(-63.18, -17.78);
        mapView.Map.Navigator.CenterOn(new Mapsui.MPoint(x, y));
        mapView.Map.Navigator.ZoomToLevel(14);
    }

    private void OnToggleFiltersClicked(object sender, EventArgs e)
    {
        FiltersPanel.IsVisible = !FiltersPanel.IsVisible;
    }
}
