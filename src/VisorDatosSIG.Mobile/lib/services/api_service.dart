import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/inmueble.dart';

class ApiService {
  // Por defecto localhost:5000 para Windows Desktop / Web, o 10.0.2.2:5000 para Android Emulator
  static String baseUrl = 'http://localhost:5000';
  static String? sessionCookie;

  static void setBaseUrl(String url) {
    baseUrl = url.trim().replaceAll(RegExp(r'/$'), '');
  }

  static Future<bool> login(String usuario, String password) async {
    try {
      // 1. Obtener cookie y token CSRF
      final getRes = await http.get(Uri.parse('$baseUrl/Account/Login'));
      final rawCookies = getRes.headers['set-cookie'];
      final body = getRes.body;
      
      final tokenMatch = RegExp(r'name="__RequestVerificationToken" type="hidden" value="([^"]+)"')
          .firstMatch(body);
      final token = tokenMatch != null ? tokenMatch.group(1) : '';

      // 2. Enviar credenciales
      final headers = <String, String>{
        'Content-Type': 'application/x-www-form-urlencoded',
      };
      if (rawCookies != null) {
        headers['Cookie'] = rawCookies;
      }

      final formBody = <String, String>{
        'Usuario': usuario,
        'Password': password,
      };
      if (token != null && token.isNotEmpty) {
        formBody['__RequestVerificationToken'] = token;
      }

      final postRes = await http.post(
        Uri.parse('$baseUrl/Account/Login'),
        headers: headers,
        body: formBody,
      );

      final loginCookies = postRes.headers['set-cookie'] ?? rawCookies;
      if (loginCookies != null) {
        sessionCookie = loginCookies;
      }
      return postRes.statusCode == 302 || postRes.statusCode == 200;
    } catch (e) {
      return false;
    }
  }

  static Future<List<Inmueble>> buscarInmuebles({
    String? texto,
    String? uv,
    String? mza,
    String? tipoPredio = 'TODOS',
  }) async {
    try {
      final params = <String, String>{};
      if (texto != null && texto.isNotEmpty) params['texto'] = texto;
      if (uv != null && uv.isNotEmpty) params['uv'] = uv;
      if (mza != null && mza.isNotEmpty) params['mza'] = mza;
      if (tipoPredio != null && tipoPredio.isNotEmpty) params['tipoPredio'] = tipoPredio;

      final uri = Uri.parse('$baseUrl/api/busqueda').replace(queryParameters: params);
      final headers = <String, String>{};
      if (sessionCookie != null) {
        headers['Cookie'] = sessionCookie!;
      }

      final res = await http.get(uri, headers: headers);

      if (res.statusCode == 200) {
        final List<dynamic> data = jsonDecode(res.body);
        return data.map((item) => Inmueble.fromJson(item)).toList();
      }
    } catch (_) {}
    return [];
  }

  static Future<Map<String, dynamic>?> obtenerEstadisticas() async {
    try {
      final headers = <String, String>{};
      if (sessionCookie != null) {
        headers['Cookie'] = sessionCookie!;
      }
      final res = await http.get(
        Uri.parse('$baseUrl/api/estadisticas'),
        headers: headers,
      );
      if (res.statusCode == 200) {
        return jsonDecode(res.body) as Map<String, dynamic>;
      }
    } catch (_) {}
    return null;
  }
}
