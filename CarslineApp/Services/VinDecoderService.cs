using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CarslineApp.Services
{

    public class VinDecoderService
    {
        private const string NhtsaUrl =
            "https://vpic.nhtsa.dot.gov/api/vehicles/DecodeVinValues/{0}?format=json";

        private static readonly HttpClient _http = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        public async Task<VinDecodedResult> DecodificarVinAsync(string vin)
        {
            vin = vin?.Trim().ToUpper() ?? string.Empty;

            if (vin.Length != 17)
                return Error(vin, $"El VIN debe tener 17 caracteres (tiene {vin.Length})");

            if (vin.Any(c => c == 'I' || c == 'O' || c == 'Q'))
                return Error(vin, "El VIN no puede contener las letras I, O ni Q");

            // ── 1. Intentar NHTSA ─────────────────────────────────────────
            VinDecodedResult? resultado = null;
            try
            {
                resultado = await DecodificarNhtsaAsync(vin);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VIN] NHTSA falló: {ex.Message}");
            }

            // ── 2. Fallback local si NHTSA no dio marca ───────────────────
            if (resultado == null || !resultado.MarcaReconocida)
            {
                var local = DecodificarLocal(vin);

                if (resultado == null)
                {
                    // NHTSA no respondió — usar todo lo local
                    resultado = local;
                    resultado.FuenteDatos = "Local (sin conexión)";
                }
                else
                {
                    // NHTSA respondió pero sin marca — completar con local
                    if (!resultado.MarcaReconocida && local.MarcaReconocida)
                    {
                        resultado.Marca = local.Marca;
                        resultado.MarcaReconocida = true;
                    }
                    if (!resultado.AnioReconocido && local.AnioReconocido)
                    {
                        resultado.Anio = local.Anio;
                        resultado.AnioReconocido = true;
                    }
                    resultado.FuenteDatos = "NHTSA + Local";
                }
            }
            else
            {
                resultado.FuenteDatos = "NHTSA";
            }

            resultado.DecodificadoCorrectamente = resultado.MarcaReconocida;
            return resultado;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  NHTSA
        // ══════════════════════════════════════════════════════════════════════

        private async Task<VinDecodedResult?> DecodificarNhtsaAsync(string vin)
        {
            var url = string.Format(NhtsaUrl, vin);
            var response = await _http.GetFromJsonAsync<NhtsaResponse>(url);

            if (response?.Results == null || response.Results.Length == 0)
                return null;

            var r = response.Results[0];

            // NHTSA regresa ErrorCode "0" = OK, cualquier otro = problema
            // "8" o "11" suelen ser VIN no encontrado
            var marcaNhtsa = Limpiar(r.Make);
            var anioNhtsa = ParseInt(r.ModelYear);

            System.Diagnostics.Debug.WriteLine(
                $"[VIN] NHTSA → ErrorCode:{r.ErrorCode} Make:{marcaNhtsa} Year:{anioNhtsa}");

            return new VinDecodedResult
            {
                VIN = vin,
                Marca = marcaNhtsa,
                Modelo = Limpiar(r.Model),
                Anio = anioNhtsa,
                NumCilindros = Limpiar(r.EngineCylinders),
                Displacement = Limpiar(r.DisplacementL),
                TipoCombust = Limpiar(r.FuelTypePrimary),
                Transmision = Limpiar(r.TransmissionStyle),
                TraccionTipo = Limpiar(r.DriveType),
                NumPuertas = Limpiar(r.Doors),
                PaisOrigen = Limpiar(r.PlantCountry),
                TipoVehiculo = Limpiar(r.VehicleType),
                MarcaReconocida = !string.IsNullOrEmpty(marcaNhtsa),
                AnioReconocido = anioNhtsa > 1980,
            };
        }
        private static VinDecodedResult DecodificarLocal(string vin)
        {
            var wmi = vin[..3];
            var marca = _wmiMarca.TryGetValue(wmi, out var m) ? m : string.Empty;

            var anioChar = char.ToUpper(vin[9]);
            var anio = _anioTabla.TryGetValue(anioChar, out var a) ? a : 0;

            // Ajuste de ciclo: si el año calculado es futuro lejano → ciclo anterior
            if (anio > DateTime.Now.Year + 1)
                anio -= 30;

            System.Diagnostics.Debug.WriteLine(
                $"[VIN] Local → WMI:{wmi} Marca:{marca} Año:{anio}");

            return new VinDecodedResult
            {
                VIN = vin,
                Marca = marca,
                Anio = anio,
                MarcaReconocida = !string.IsNullOrEmpty(marca),
                AnioReconocido = anio > 1980,
                FuenteDatos = "Local",
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TABLA WMI → MARCA  (ISO 3779 — actualizada para México)
        // ══════════════════════════════════════════════════════════════════════

        private static readonly Dictionary<string, string> _wmiMarca =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // ── México (prefijo 3) ────────────────────────────────────────────
                ["3VW"] = "Volkswagen",   // Planta Puebla
                ["3VF"] = "Volkswagen",
                ["3VE"] = "Volkswagen",
                ["3N1"] = "Nissan",       // CIVAC Morelos / Aguascalientes
                ["3N6"] = "Nissan",
                ["3N8"] = "Nissan",
                ["3G1"] = "Chevrolet",
                ["3G3"] = "Chevrolet",
                ["3G5"] = "Chevrolet",
                ["3GN"] = "Chevrolet",
                ["3GY"] = "Cadillac",
                ["3FA"] = "Ford",         // Planta Hermosillo
                ["3FE"] = "Ford",
                ["3FM"] = "Ford",
                ["3FP"] = "Ford",
                ["3HG"] = "Honda",        // Planta El Salto, Jalisco
                ["3HH"] = "Honda",
                ["3HM"] = "Honda",
                ["3TM"] = "Toyota",       // Planta Apaseo el Grande
                ["3TY"] = "Toyota",
                ["3TB"] = "Toyota",
                ["3KP"] = "Kia",          // Planta Pesquería, NL
                ["3KN"] = "Kia",
                ["3MZ"] = "Mazda",        // Planta Salamanca, Gto
                ["3MY"] = "Mazda",
                ["3AU"] = "Audi",         // Planta San José Chiapa, Puebla
                ["3A8"] = "Audi",
                ["3BM"] = "BMW",          // Planta San Luis Potosí
                ["3MH"] = "Mitsubishi",
                ["3C1"] = "Dodge",
                ["3C3"] = "Dodge",
                ["3C4"] = "Jeep",
                ["3C6"] = "RAM",
                ["3C7"] = "RAM",
                ["3D3"] = "Dodge",
                ["3D4"] = "Dodge",
                ["3IG"] = "Isuzu",
                // ── EE.UU. (prefijos 1 y 4) ──────────────────────────────────────
                ["1FA"] = "Ford",
                ["1FB"] = "Ford",
                ["1FC"] = "Ford",
                ["1FD"] = "Ford",
                ["1FM"] = "Ford",
                ["1FT"] = "Ford",
                ["4F1"] = "Ford",
                ["4F2"] = "Ford",
                ["1G1"] = "Chevrolet",
                ["1G4"] = "Buick",
                ["1G6"] = "Cadillac",
                ["1GC"] = "Chevrolet",
                ["1GD"] = "Chevrolet",
                ["1GE"] = "Chevrolet",
                ["1GK"] = "GMC",
                ["1GT"] = "GMC",
                ["1GY"] = "Cadillac",
                ["1B3"] = "Dodge",
                ["1B4"] = "Dodge",
                ["1C3"] = "Chrysler",
                ["1C4"] = "Jeep",
                ["1C6"] = "RAM",
                ["1D3"] = "Dodge",
                ["1D7"] = "Dodge",
                ["1HG"] = "Honda",
                ["1HF"] = "Honda",
                ["5FN"] = "Honda",
                ["5J6"] = "Honda",
                ["5J8"] = "Honda",
                ["19X"] = "Honda",
                ["1N4"] = "Nissan",
                ["1N6"] = "Nissan",
                ["5N1"] = "Nissan",
                ["1J4"] = "Jeep",
                ["1J8"] = "Jeep",
                ["4T1"] = "Toyota",
                ["4T3"] = "Toyota",
                ["4T4"] = "Toyota",
                ["5TD"] = "Toyota",
                ["5TE"] = "Toyota",
                ["5TF"] = "Toyota",
                ["5YJ"] = "Tesla",
                ["7SA"] = "Tesla",
                ["5LM"] = "Lincoln",
                ["5NP"] = "Hyundai",
                ["5NM"] = "Hyundai",
                ["5XX"] = "Kia",
                ["1VW"] = "Volkswagen",
                ["4US"] = "BMW",
                ["5UX"] = "BMW",
                ["4JG"] = "Mercedes-Benz",
                ["4S3"] = "Subaru",
                ["4S4"] = "Subaru",
                // ── Canadá (prefijo 2) ────────────────────────────────────────────
                ["2FA"] = "Ford",
                ["2FM"] = "Ford",
                ["2G1"] = "Chevrolet",
                ["2GT"] = "GMC",
                ["2HG"] = "Honda",
                ["2HK"] = "Honda",
                ["2T1"] = "Toyota",
                ["2T2"] = "Lexus",
                ["2T3"] = "Toyota",
                // ── Japón (prefijo J) ─────────────────────────────────────────────
                ["JT2"] = "Toyota",
                ["JT3"] = "Toyota",
                ["JT4"] = "Toyota",
                ["JTD"] = "Toyota",
                ["JTE"] = "Toyota",
                ["JTG"] = "Toyota",
                ["JTN"] = "Toyota",
                ["JT6"] = "Lexus",
                ["JTH"] = "Lexus",
                ["JTJ"] = "Lexus",
                ["JTK"] = "Lexus",
                ["JN1"] = "Nissan",
                ["JN3"] = "Nissan",
                ["JN6"] = "Nissan",
                ["JN8"] = "Nissan",
                ["JNK"] = "Infiniti",
                ["JHM"] = "Honda",
                ["JH4"] = "Acura",
                ["JM1"] = "Mazda",
                ["JM6"] = "Mazda",
                ["JA3"] = "Mitsubishi",
                ["JA4"] = "Mitsubishi",
                ["JF1"] = "Subaru",
                ["JF2"] = "Subaru",
                ["JS1"] = "Suzuki",
                ["JS2"] = "Suzuki",
                ["JS3"] = "Suzuki",
                // ── Corea (prefijo K) ─────────────────────────────────────────────
                ["KMH"] = "Hyundai",
                ["KMF"] = "Hyundai",
                ["KMJ"] = "Hyundai",
                ["KNA"] = "Kia",
                ["KND"] = "Kia",
                // ── Alemania (prefijo W) ──────────────────────────────────────────
                ["WBA"] = "BMW",
                ["WBS"] = "BMW",
                ["WBY"] = "BMW",
                ["WMW"] = "Mini",
                ["WME"] = "Smart",
                ["WVW"] = "Volkswagen",
                ["WV1"] = "Volkswagen",
                ["WV2"] = "Volkswagen",
                ["WAU"] = "Audi",
                ["WUA"] = "Audi",
                ["WP0"] = "Porsche",
                ["WP1"] = "Porsche",
                ["WDB"] = "Mercedes-Benz",
                ["WDD"] = "Mercedes-Benz",
                ["WDC"] = "Mercedes-Benz",
                ["WDF"] = "Mercedes-Benz",
                ["W0L"] = "Opel",
                // ── Reino Unido (prefijo S) ───────────────────────────────────────
                ["SAL"] = "Land Rover",
                ["SAJ"] = "Jaguar",
                ["SCA"] = "Rolls-Royce",
                ["SCB"] = "Bentley",
                ["SCF"] = "Aston Martin",
                // ── Francia ───────────────────────────────────────────────────────
                ["VF1"] = "Renault",
                ["VF3"] = "Peugeot",
                ["VF7"] = "Citroën",
                // ── Italia ────────────────────────────────────────────────────────
                ["ZFA"] = "Fiat",
                ["ZFF"] = "Ferrari",
                ["ZHW"] = "Lamborghini",
                ["ZAR"] = "Alfa Romeo",
                // ── Suecia ────────────────────────────────────────────────────────
                ["YV1"] = "Volvo",
                ["YV4"] = "Volvo",
                // ── Corea / otros ─────────────────────────────────────────────────
                ["VSS"] = "SEAT",
                ["TMB"] = "Škoda",
            };

        // ══════════════════════════════════════════════════════════════════════
        //  TABLA AÑO (posición 10 del VIN — ISO 3779)
        // ══════════════════════════════════════════════════════════════════════

        private static readonly Dictionary<char, int> _anioTabla = new()
        {
            ['A'] = 2010,
            ['B'] = 2011,
            ['C'] = 2012,
            ['D'] = 2013,
            ['E'] = 2014,
            ['F'] = 2015,
            ['G'] = 2016,
            ['H'] = 2017,
            ['J'] = 2018,
            ['K'] = 2019,
            ['L'] = 2020,
            ['M'] = 2021,
            ['N'] = 2022,
            ['P'] = 2023,
            ['R'] = 2024,
            ['S'] = 2025,
            ['T'] = 2026,
            ['V'] = 2027,
            ['W'] = 2028,
            ['X'] = 2029,
            ['Y'] = 2030,
            ['1'] = 2001,
            ['2'] = 2002,
            ['3'] = 2003,
            ['4'] = 2004,
            ['5'] = 2005,
            ['6'] = 2006,
            ['7'] = 2007,
            ['8'] = 2008,
            ['9'] = 2009,
        };

        // ══════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════════════

        private static string Limpiar(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();

        private static int ParseInt(string? texto) =>
            int.TryParse(texto, out int n) && n > 1980 ? n : 0;

        private static VinDecodedResult Error(string vin, string msg) => new()
        {
            VIN = vin,
            ErrorTexto = msg,
            DecodificadoCorrectamente = false
        };

        // ══════════════════════════════════════════════════════════════════════
        //  MODELOS INTERNOS — respuesta NHTSA
        // ══════════════════════════════════════════════════════════════════════

        private class NhtsaResponse
        {
            [JsonPropertyName("Results")]
            public NhtsaVehicle[]? Results { get; set; }
        }

        private class NhtsaVehicle
        {
            [JsonPropertyName("ErrorCode")] public string? ErrorCode { get; set; }
            [JsonPropertyName("ErrorText")] public string? ErrorText { get; set; }
            [JsonPropertyName("Make")] public string? Make { get; set; }
            [JsonPropertyName("Model")] public string? Model { get; set; }
            [JsonPropertyName("ModelYear")] public string? ModelYear { get; set; }
            [JsonPropertyName("VehicleType")] public string? VehicleType { get; set; }
            [JsonPropertyName("EngineCylinders")] public string? EngineCylinders { get; set; }
            [JsonPropertyName("DisplacementL")] public string? DisplacementL { get; set; }
            [JsonPropertyName("FuelTypePrimary")] public string? FuelTypePrimary { get; set; }
            [JsonPropertyName("TransmissionStyle")] public string? TransmissionStyle { get; set; }
            [JsonPropertyName("DriveType")] public string? DriveType { get; set; }
            [JsonPropertyName("Doors")] public string? Doors { get; set; }
            [JsonPropertyName("PlantCountry")] public string? PlantCountry { get; set; }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  RESULTADO — compatible con el ViewModel existente
    // ══════════════════════════════════════════════════════════════════════════

    public class VinDecodedResult
    {
        public string VIN { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string TipoVehiculo { get; set; } = string.Empty;
        public string NumCilindros { get; set; } = string.Empty;
        public string Displacement { get; set; } = string.Empty;
        public string TipoCombust { get; set; } = string.Empty;
        public string Transmision { get; set; } = string.Empty;
        public string TraccionTipo { get; set; } = string.Empty;
        public string NumPuertas { get; set; } = string.Empty;
        public string PaisOrigen { get; set; } = string.Empty;
        public string? ErrorTexto { get; set; }
        public string FuenteDatos { get; set; } = string.Empty;  // "NHTSA" | "Local" | "NHTSA + Local"

        public bool MarcaReconocida { get; set; }
        public bool AnioReconocido { get; set; }
        public bool DecodificadoCorrectamente { get; set; }

        public string ResumenVehiculo => $"{Anio} {Marca} {Modelo}".Trim();
    }
}