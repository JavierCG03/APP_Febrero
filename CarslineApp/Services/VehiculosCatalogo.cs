using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarslineApp.Services
{
    /// <summary>
    /// Catálogo local de vehículos: Marca → Modelo → Versiones.
    ///   var marcas  = VehicleCatalogService.ObtenerMarcas();
    ///   var modelos = VehicleCatalogService.ObtenerModelos("Nissan");
    ///   var versiones = VehicleCatalogService.ObtenerVersiones("Nissan", "Versa");
    /// </summary>
    public static class VehiculosCatalogo
    {
        // ══════════════════════════════════════════════════════════════════════
        //  CATÁLOGO  Marca → Modelo → Lista de Versiones
        // ══════════════════════════════════════════════════════════════════════
        private static readonly Dictionary<string, Dictionary<string, List<string>>> _catalogo =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // ─────────────────────────────────────────────────────────────────
                ["Chevrolet"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Aveo"] = new() { "LS", "LT", "Premier" },
                    ["Beat"] = new() { "LS", "LT", "RS", "Premier" },
                    ["Blazer"] = new() { "RS", "Premier", "RS AWD" },
                    ["Captiva"] = new() { "LS", "LT", "Premier" },
                    ["Cavalier"] = new() { "LS", "LT", "RS", "Premier" },
                    ["Colorado"] = new() { "LT", "Z71", "ZR2" },
                    ["Cruze"] = new() { "LS", "LT", "Premier" },
                    ["Equinox"] = new() { "LS", "LT", "RS", "Premier" },
                    ["Malibu"] = new() { "LS", "LT", "Premier" },
                    ["Onix"] = new() { "LS", "LT", "RS", "Premier" },
                    ["Silverado"] = new() { "WT", "LT", "RST", "LTZ", "High Country", "ZR2" },
                    ["Sonic"] = new() { "LS", "LT", "RS", "Premier" },
                    ["Spark"] = new() { "LS", "LT", "Premier" },
                    ["Tahoe"] = new() { "LS", "LT", "RST", "Premier", "High Country" },
                    ["Trailblazer"] = new() { "LS", "LT", "RS", "Premier", "Activ" },
                    ["Trax"] = new() { "LS", "LT", "Premier", "RS", "Activ" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Ford"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Bronco"] = new() { "Base", "Big Bend", "Black Diamond", "Badlands", "Outer Banks", "Wildtrak", "Raptor" },
                    ["EcoSport"] = new() { "S", "SE", "Titanium", "Storm" },
                    ["Edge"] = new() { "SE", "SEL", "ST", "Titanium" },
                    ["Escape"] = new() { "S", "SE", "SEL", "Titanium", "PHEV" },
                    ["Explorer"] = new() { "Base", "XLT", "Limited", "ST", "Timberline", "Platinum" },
                    ["F-150"] = new() { "XL", "XLT", "Lariat", "King Ranch", "Platinum", "Limited", "Raptor" },
                    ["F-250"] = new() { "XL", "XLT", "Lariat", "King Ranch", "Platinum" },
                    ["Fiesta"] = new() { "S", "SE", "Titanium", "ST" },
                    ["Focus"] = new() { "S", "SE", "SEL", "Titanium", "ST" },
                    ["Fusion"] = new() { "S", "SE", "SEL", "Titanium", "Energi" },
                    ["Mustang"] = new() { "EcoBoost", "EcoBoost Premium", "GT", "GT Premium", "Mach 1", "Shelby GT500" },
                    ["Ranger"] = new() { "XL", "XLT", "Lariat", "Tremor", "Raptor" },
                    ["Territory"] = new() { "Trend", "SEL", "Titanium" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Nissan"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Altima"] = new() { "Sense", "Advance", "Exclusive", "SR" },
                    ["Frontier"] = new() { "S", "SV", "PRO-4X", "Pro-X" },
                    ["Kicks"] = new() { "Sense", "Advance", "Exclusive", "SR" },
                    ["Leaf"] = new() { "S", "SV", "SV Plus", "SL Plus" },
                    ["March"] = new() { "Sense", "Advance", "SR", "Nismo" },
                    ["Maxima"] = new() { "SV", "SR", "Platinum" },
                    ["Murano"] = new() { "S", "SV", "SL", "Platinum" },
                    ["NP300"] = new() { "SE", "XE", "LE", "Platinum", "Pro-4X" },
                    ["Pathfinder"] = new() { "S", "SV", "SL", "Platinum" },
                    ["Qashqai"] = new() { "Sense", "Advance", "Exclusive" },
                    ["Sentra"] = new() { "Sense", "Advance", "SR", "Exclusive" },
                    ["Tiida"] = new() { "Sense", "Advance", "Exclusive" },
                    ["Versa"] = new() { "Sense", "Advance", "Exclusive" },
                    ["X-Trail"] = new() { "Sense", "Advance", "Exclusive", "Platinum" },
                    ["Urvan"] = new() { "Panel","Panel Ventanas","11 Pasajeros", "14 Pasajeros"},
                },
                // ─────────────────────────────────────────────────────────────────
                ["Toyota"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["4Runner"] = new() { "SR5", "TRD Off-Road", "TRD Pro", "Limited", "Venture" },
                    ["Camry"] = new() { "LE", "SE", "XLE", "XSE", "TRD" },
                    ["Corolla"] = new() { "Base CVT", "LE CVT", "XLE CVT", "SE CVT", "LE HEV", "XLE HEV" },
                    ["Fortuner"] = new() { "SR", "SR5", "SRV", "GR-S" },
                    ["Hilux"] = new() { "DX", "SR", "SRV", "GR-S" },
                    ["Land Cruiser"] = new() { "GX", "VX", "GR Sport" },
                    ["Prius"] = new() { "LE", "XLE", "Limited", "Prime LE", "Prime XLE" },
                    ["RAV4"] = new() { "LE", "XLE", "XLE Premium", "Adventure", "Limited", "Hybrid LE", "Hybrid XLE", "Hybrid Limited" },
                    ["Sequoia"] = new() { "SR5", "Limited", "Platinum", "TRD Pro", "Capstone" },
                    ["Sienna"] = new() { "LE", "XLE", "XSE", "Limited", "Platinum" },
                    ["Tacoma"] = new() { "SR", "SR5", "TRD Sport", "TRD Off-Road", "Limited", "TRD Pro" },
                    ["Yaris"] = new() { "S", "R", "S CVT", "GR Sport" },
                    ["Hiace"] = new(){ "Panel", "Panel Ventanas","Panel Superlarga","Ventanas Superlarga","12 Pasajeros"},
                    ["Tundra"] = new(){"Limited","Platinum", "TRD Off Road HEV","TRD Pro HEV"},
                },
                // ─────────────────────────────────────────────────────────────────
                ["Lexus"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["UX"] = new() { "UX 200", "UX 250h" },
                    ["IS"] = new() { "IS 300", "IS 350 F SPORT" },
                    ["ES"] = new() { "ES 250", "ES 300h", "ES 350" },
                    ["NX"] = new() { "NX 250", "NX 350", "NX 350h", "NX 450h+" },
                    ["RX"] = new() { "RX 350", "RX 350h", "RX 500h F SPORT" },
                    ["TX"] = new() { "TX 350h", "TX 500h F SPORT" },
                    ["GX"] = new() { "GX 460", "GX 550 Overtrail", "GX 550 Overtrail Plus" },
                    ["LX"] = new() { "LX 600", "LX 700h Luxury" },
                    ["LC"] = new() { "LC 500", "LC 500h" },
                    ["LS"] = new() { "LS 500", "LS 500h" },
                    ["RZ"] = new() { "RZ 450e", "RZ 550e F SPORT" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Volkswagen"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Bora"] = new() { "Trendline", "Comfortline", "Highline", "GLI" },
                    ["Crossfox"] = new() { "Trendline", "Comfortline" },
                    ["Gol"] = new() { "Trendline", "Comfortline" },
                    ["Golf"] = new() { "Trendline", "Comfortline", "Highline", "GTI", "R" },
                    ["Jetta"] = new() { "Trendline", "Comfortline", "Highline", "GLI", "SEL" },
                    ["Passat"] = new() { "Trendline", "Comfortline", "Highline" },
                    ["Polo"] = new() { "Trendline", "Comfortline", "Highline" },
                    ["Saveiro"] = new() { "Trendline", "Comfortline", "Cross" },
                    ["Taos"] = new() { "Trendline", "Comfortline", "Highline" },
                    ["Tiguan"] = new() { "Trendline", "Comfortline", "Highline", "R-Line" },
                    ["Touareg"] = new() { "Premium", "R-Line", "Elegance" },
                    ["Vento"] = new() { "Trendline", "Comfortline", "Highline", "GLI" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Honda"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Accord"] = new() { "LX", "Sport", "EX-L", "Touring", "Hybrid" },
                    ["BR-V"] = new() { "Prime", "Uniq", "Epic" },
                    ["City"] = new() { "Uniq", "Prime", "Epic", "Hatchback" },
                    ["Civic"] = new() { "Sport", "i-Style", "Touring", "Si", "Type R" },
                    ["CR-V"] = new() { "Turbo", "Turbo Plus", "Touring", "Hybrid" },
                    ["Fit"] = new() { "Fun", "Hit", "EX" },
                    ["HR-V"] = new() { "Prime", "Epic", "Touring" },
                    ["Odyssey"] = new() { "LX", "EX", "EX-L", "Touring", "Elite" },
                    ["Pilot"] = new() { "Sport", "EX-L", "TrailSport", "Elite" },
                    ["Ridgeline"] = new() { "Sport", "RTL", "RTL-E", "Black Edition" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Hyundai"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Accent"] = new() { "GL", "GLS", "GLS Mid", "GLS Premium" },
                    ["Creta"] = new() { "GL", "GLS", "Limited" },
                    ["Elantra"] = new() { "GLS", "Limited", "N Line" },
                    ["Ioniq"] = new() { "Hybrid GLS", "Plug-in GLS", "Electric GLS" },
                    ["Kona"] = new() { "GLS", "Limited", "N Line", "Electric" },
                    ["Santa Fe"] = new() { "GLS", "Limited", "Limited 2.5T" },
                    ["Sonata"] = new() { "GLS", "Limited", "N Line" },
                    ["Tucson"] = new() { "GLS", "Limited", "N Line" },
                    ["Venue"] = new() { "GL", "GLS", "Limited" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Kia"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Carnival"] = new() { "LX", "EX", "SX", "SX Prestige" },
                    ["EV6"] = new() { "Wind", "GT-Line", "GT-Line AWD" },
                    ["K3"] = new() { "EX", "EX Premium", "GT Line" },
                    ["K4"] = new() { "L MT", "L IVT", "LX MT", "LX IVT","EX IVT","GT Line","GT Line Turbo"},
                    ["K5"] = new() { "LX", "EX", "GT-Line", "EX Premium" },
                    ["Niro"] = new() { "LX Hybrid", "EX Hybrid", "EX Premium Hybrid", "EV LX", "EV EX" },
                    ["Rio"] = new() { "LX", "EX", "GT Line" },
                    ["Seltos"] = new() { "LX", "EX", "EX Premium", "SX" },
                    ["Sorento"] = new() { "LX", "EX", "SX", "SX Prestige" },
                    ["Soul"] = new() { "LX", "EX", "GT-Line", "Turbo" },
                    ["Sportage"] = new() { "LX", "EX", "EX Premium", "SX" },
                    ["Stinger"] = new() { "GT-Line", "GT1", "GT2" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Mazda"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["CX-3"] = new() { "i Sport", "i Grand Touring", "s Grand Touring" },
                    ["CX-30"] = new() { "i Sport", "i SkyActiv", "i Grand Touring", "s Grand Touring", "Turbo" },
                    ["CX-5"] = new() { "i Sport", "i SkyActiv", "i Grand Touring", "s Grand Touring", "Turbo", "Signature" },
                    ["CX-7"] = new() { "i Sport", "i Touring", "i Grand Touring" },
                    ["CX-9"] = new() { "Sport", "Touring", "Grand Touring", "Signature" },
                    ["CX-50"] = new() { "i Sport", "i Grand Touring", "Turbo", "Turbo Meridian Edition" },
                    ["CX-60"] = new() { "Prime-Line", "Exclusive-Line", "Homura", "Takumi" },
                    ["CX-70"] = new() { "Preferred", "Premium", "Premium Plus" },
                    ["CX-90"] = new() { "Preferred", "Premium", "Premium Plus", "Turbo S", "Turbo S Premium Plus" },
                    ["Mazda2"] = new() { "i Sport", "i Grand Touring", "Signature" },
                    ["Mazda3"] = new() { "i Sport", "i SkyActiv", "i Grand Touring", "s Grand Touring", "Turbo", "Signature" },
                    ["Mazda5"] = new() { "Sport", "Touring", "Grand Touring" },
                    ["Mazda6"] = new() { "i Sport", "i Grand Touring", "s Grand Touring", "Signature" },
                    ["MX-5"] = new() { "Sport", "Club", "Grand Touring", "RF Club", "RF Grand Touring" },
                    ["MX-30"] = new() { "e-SkyActiv", "Premium Plus" },
                    ["BT-50"] = new() { "4x2", "4x4", "Pro", "High" }
                },
                // ─────────────────────────────────────────────────────────────────
                ["Dodge"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Attitude"] = new() { "SE", "SXT", "Sport", "GT" },
                    ["Challenger"] = new() { "SXT", "GT", "R/T", "R/T Scat Pack", "SRT 392", "SRT Hellcat", "SRT Demon" },
                    ["Charger"] = new() { "SXT", "GT", "R/T", "Scat Pack", "SRT 392", "SRT Hellcat", "SRT Hellcat Redeye" },
                    ["Durango"] = new() { "SXT", "GT", "R/T", "Citadel", "SRT 392", "SRT Hellcat" },
                    ["Journey"] = new() { "SE", "SXT", "Crossroad", "GT" },
                    ["Neon"] = new() { "SE", "SXT", "GT", "Sport" },
                    ["Vision"] = new() { "SE", "SXT" },
                    ["Grand Caravan"] = new() { "SE", "SXT", "GT" },
                    ["Dakota"] = new() { "SLT", "Sport", "Laramie" },
                    ["Viper"] = new() { "SRT-10", "GTS", "ACR" },
                    ["Nitro"] = new() { "SE", "SXT", "SLT", "R/T" },
                    ["Caliber"] = new() { "SE", "SXT", "R/T", "SRT4" },
                    ["Avenger"] = new() { "SE", "SXT", "R/T" },
                    ["i10"] = new() { "GL", "GLS" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Jeep"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Cherokee"] = new() { "Sport", "Latitude", "Trailhawk", "Limited", "Overland" },
                    ["Compass"] = new() { "Sport", "Latitude", "Trailhawk", "Limited" },
                    ["Gladiator"] = new() { "Sport", "Sport S", "Overland", "Mojave", "Rubicon" },
                    ["Grand Cherokee"] = new() { "Laredo", "Altitude", "Limited", "Trailhawk", "Overland", "Summit" },
                    ["Renegade"] = new() { "Sport", "Latitude", "Trailhawk", "Limited" },
                    ["Wrangler"] = new() { "Sport", "Sport S", "Sahara", "Rubicon", "4xe" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["RAM"] = new(StringComparer.OrdinalIgnoreCase)
                {

                    ["700"] = new() { "SLT Cabina Regular", "SLT Cabina Doble", "Big Horn", "Big Horn CVT", "Laramie", "Laramie CVT Turbo" },
                    ["1200"] = new() { "Tradesman Regular Cab", "Tradesman Doble Cab", "Big Horn", "Laramie" },
                    ["1500"] = new() { "Tradesman", "Express", "Big Horn", "Rebel", "Laramie", "Longhorn", "Limited", "TRX", "RHO" },
                    ["2500"] = new() { "Tradesman", "Big Horn", "Power Wagon", "Laramie", "Longhorn", "Limited" },
                    ["2500"] = new() { "Tradesman", "Big Horn", "Power Wagon", "Laramie", "Longhorn", "Limited" },
                    ["ProMaster"] = new() { "1500 Cargo Van", "2500 Cargo Van", "3500 Cargo Van", "2500 Window Van", "3500 Window Van", "City Cargo Van", "City Wagon" },
                    ["3500"] = new() { "Tradesman", "Big Horn", "Laramie", "Power Wagon", "Limited Longhorn", "Limited" },
                },

                // ─────────────────────────────────────────────────────────────────
                ["Audi"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["A1"] = new() { "30 TFSI", "35 TFSI", "S1" },
                    ["A3"] = new() { "35 TFSI", "40 TFSI", "45 TFSI e", "S3" },
                    ["A4"] = new() { "35 TFSI", "40 TFSI", "45 TFSI", "S4" },
                    ["A5"] = new() { "40 TFSI", "45 TFSI", "S5" },
                    ["A6"] = new() { "40 TFSI", "45 TFSI", "55 TFSI", "S6" },
                    ["Q2"] = new() { "35 TFSI", "40 TFSI" },
                    ["Q3"] = new() { "35 TFSI", "40 TFSI", "45 TFSI e", "RS Q3" },
                    ["Q5"] = new() { "40 TFSI", "45 TFSI", "55 TFSI e", "SQ5" },
                    ["Q7"] = new() { "45 TFSI", "55 TFSI", "SQ7" },
                    ["Q8"] = new() { "55 TFSI", "SQ8", "RS Q8", "e-tron" },
                    ["TT"] = new() { "40 TFSI", "45 TFSI", "TTS" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["BMW"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["116i"] = new() { "Advantage", "Sport Line", "M Sport" },
                    ["118i"] = new() { "Advantage", "Sport Line", "M Sport" },
                    ["218i"] = new() { "Advantage", "Sport Line", "M Sport" },
                    ["320i"] = new() { "Advantage", "Sport Line", "M Sport", "xDrive" },
                    ["330i"] = new() { "M Sport", "xDrive", "M Sport xDrive" },
                    ["420i"] = new() { "Advantage", "Sport Line", "M Sport" },
                    ["520i"] = new() { "Advantage", "Sport Line", "M Sport" },
                    ["530i"] = new() { "M Sport", "xDrive M Sport" },
                    ["X1"] = new() { "sDrive18i", "sDrive20i", "xDrive20i M Sport" },
                    ["X2"] = new() { "sDrive18i", "sDrive20i", "xDrive20i M Sport" },
                    ["X3"] = new() { "sDrive20i", "xDrive20i", "xDrive30i", "M40i" },
                    ["X4"] = new() { "xDrive20i", "xDrive30i", "M40i" },
                    ["X5"] = new() { "xDrive40i", "xDrive45e", "M50i", "xDrive50e" },
                    ["X6"] = new() { "xDrive40i", "M50i" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Mercedes-Benz"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["A 200"] = new() { "Progressive", "AMG Line", "AMG" },
                    ["B 200"] = new() { "Progressive", "AMG Line" },
                    ["C 180"] = new() { "Avantgarde", "AMG Line" },
                    ["C 200"] = new() { "Avantgarde", "AMG Line", "AMG" },
                    ["CLA"] = new() { "200 Progressive", "200 AMG Line", "AMG 35" },
                    ["E 200"] = new() { "Avantgarde", "AMG Line" },
                    ["GLA"] = new() { "200 Progressive", "200 AMG Line", "AMG 35" },
                    ["GLB"] = new() { "200 Progressive", "200 AMG Line", "AMG 35" },
                    ["GLC"] = new() { "300 Avantgarde", "300 AMG Line", "AMG 43", "AMG 63" },
                    ["GLE"] = new() { "350 Avantgarde", "350 AMG Line", "AMG 53", "AMG 63" },
                    ["GLS"] = new() { "450 AMG Line", "AMG 63" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Peugeot"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["208"] = new() { "Active", "Allure", "GT Line", "GT" },
                    ["2008"] = new() { "Active", "Allure", "GT Line", "GT" },
                    ["3008"] = new() { "Active", "Allure", "GT Line", "GT" },
                    ["308"] = new() { "Active", "Allure", "GT Line", "GT" },
                    ["408"] = new() { "Active", "Allure", "GT Line" },
                    ["5008"] = new() { "Active", "Allure", "GT Line", "GT" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Renault"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Captur"] = new() { "Zen", "Intens", "Iconic" },
                    ["Duster"] = new() { "Zen", "Intens", "Iconic", "Outsider" },
                    ["Kangoo"] = new() { "Express", "Confort" },
                    ["Koleos"] = new() { "Zen", "Intens", "Iconic" },
                    ["Logan"] = new() { "Zen", "Intens" },
                    ["Oroch"] = new() { "Zen", "Intens", "Outsider" },
                    ["Sandero"] = new() { "Zen", "Stepway Zen", "Stepway Intens" },
                    ["Stepway"] = new() { "Zen", "Intens", "Iconic" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["SEAT"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Arona"] = new() { "Reference", "Style", "FR", "Xcellence" },
                    ["Ateca"] = new() { "Reference", "Style", "FR", "Xcellence" },
                    ["Ibiza"] = new() { "Reference", "Style", "FR", "Xcellence" },
                    ["Leon"] = new() { "Reference", "Style", "FR", "Xcellence", "Cupra", "FR Black Edition" },
                    ["Leon Sportstourer"] = new() { "Style", "FR", "Xcellence" },
                    ["Tarraco"] = new() { "Style", "FR", "Xcellence" },
                    ["Toledo"] = new() { "Reference", "Style", "FR", "Advance" },
                    ["Altea"] = new() { "Reference", "Style", "XL", "FR" },
                    ["Altea XL"] = new() { "Reference", "Style", "FR" },
                    ["Exeo"] = new() { "Reference", "Style", "Sport" },
                    ["Mii"] = new() { "Reference", "Style", "FR" },
                    ["Cordoba"] = new() { "Reference", "Style", "Sport" },
                    ["Freetrack"] = new() { "Style", "4Drive" }
                },
                // ─────────────────────────────────────────────────────────────────
                ["CUPRA"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Formentor"] = new() { "1.4 e-Hybrid", "2.0 TSI 190", "2.0 TSI 245", "VZ 310" },
                    ["Leon"] = new() { "1.4 e-Hybrid", "2.0 TSI 245", "VZ 300" },
                    ["Ateca"] = new() { "2.0 TSI 300" },
                    ["Born"] = new() { "58 kWh", "77 kWh" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Subaru"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["BRZ"] = new() { "Premium", "Limited", "tS" },
                    ["Crosstrek"] = new() { "Base", "Premium", "Limited", "Sport", "Wilderness" },
                    ["Forester"] = new() { "Base", "Premium", "Sport", "Limited", "Touring", "Wilderness" },
                    ["Impreza"] = new() { "Base", "Premium", "Sport", "Limited" },
                    ["Legacy"] = new() { "Base", "Premium", "Sport", "Limited", "Touring XT" },
                    ["Outback"] = new() { "Base", "Premium", "Onyx Edition", "Limited", "Touring", "Wilderness" },
                    ["WRX"] = new() { "Base", "Premium", "Limited", "GT", "STI" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Mitsubishi"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Eclipse Cross"] = new() { "ES", "SE", "SEL", "SEL Premium" },
                    ["L200"] = new() { "GLX", "GLS", "HPE", "Athlete" },
                    ["Mirage"] = new() { "ES", "LE", "SE", "SEL" },
                    ["Montero"] = new() { "Sport ES", "Sport SE", "Sport SP" },
                    ["Outlander"] = new() { "ES", "SE", "SEL", "PHEV SEL" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Fiat"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Argo"] = new() { "Drive", "Trekking", "Precision" },
                    ["Cronos"] = new() { "Drive", "Precision" },
                    ["Mobi"] = new() { "Like", "Easy", "Trekking" },
                    ["Pulse"] = new() { "Drive", "Audace", "Impetus", "Abarth" },
                    ["Toro"] = new() { "Endurance", "Freedom", "Volcano", "Ultra" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Suzuki"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Baleno"] = new() { "GL", "GLX" },
                    ["Ignis"] = new() { "GL", "GLX" },
                    ["Jimny"] = new() { "GL", "GLX", "Sierra" },
                    ["Swift"] = new() { "GL", "GLX", "Sport" },
                    ["Vitara"] = new() { "GL", "GLX", "S" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Volvo"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["S60"] = new() { "Momentum", "R-Design", "Inscription", "T8 Polestar" },
                    ["S90"] = new() { "Momentum", "Inscription", "T8" },
                    ["V60"] = new() { "Momentum", "R-Design", "Inscription", "T8" },
                    ["XC40"] = new() { "Momentum", "R-Design", "Inscription", "Recharge" },
                    ["XC60"] = new() { "Momentum", "R-Design", "Inscription", "T8", "Recharge" },
                    ["XC90"] = new() { "Momentum", "R-Design", "Inscription", "T8 Excellence" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["GMC"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Acadia"] = new() { "SLE", "SLT", "AT4", "Denali" },
                    ["Canyon"] = new() { "Base", "SLE", "Elevation", "SLT", "AT4", "Denali" },
                    ["Sierra"] = new() { "Base", "SLE", "SLT", "Elevation", "AT4", "Denali", "AT4X" },
                    ["Terrain"] = new() { "SLE", "SLT", "AT4", "Denali" },
                    ["Yukon"] = new() { "SLE", "SLT", "AT4", "Denali", "XL SLT", "XL Denali" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Buick"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Enclave"] = new() { "Preferred", "Essence", "Avenir" },
                    ["Encore"] = new() { "Preferred", "Sport Touring", "Essence", "Avenir" },
                    ["Envision"] = new() { "Preferred", "Essence", "Avenir" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Chrysler"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["300"] = new() { "Touring", "300S", "C", "SRT 392" },
                    ["Pacifica"] = new() { "Touring", "Touring L", "Limited", "Pinnacle", "Hybrid" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Mini"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Clubman"] = new() { "One", "Cooper", "Cooper S", "John Cooper Works" },
                    ["Cooper"] = new() { "One", "Cooper", "Cooper S", "John Cooper Works", "SE Electric" },
                    ["Countryman"] = new() { "One", "Cooper", "Cooper S", "Cooper SE ALL4", "John Cooper Works" },
                    ["Paceman"] = new() { "Cooper", "Cooper S", "John Cooper Works" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Porsche"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["718"] = new() { "Boxster", "Cayman", "Spyder", "GT4", "GTS 4.0" },
                    ["911"] = new() { "Carrera", "Carrera S", "Carrera 4S", "Targa 4S", "Turbo", "Turbo S", "GT3" },
                    ["Cayenne"] = new() { "Base", "S", "GTS", "Turbo", "Turbo S E-Hybrid" },
                    ["Macan"] = new() { "Base", "S", "GTS", "Turbo" },
                    ["Panamera"] = new() { "Base", "4", "4S", "GTS", "Turbo", "Turbo S E-Hybrid" },
                    ["Taycan"] = new() { "Base", "4S", "GTS", "Turbo", "Turbo S", "Cross Turismo" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Land Rover"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Defender"] = new() { "90 S", "90 SE", "90 HSE", "110 S", "110 SE", "110 HSE", "110 X" },
                    ["Discovery"] = new() { "S", "SE", "HSE", "R-Dynamic HSE", "Metropolitan" },
                    ["Discovery Sport"] = new() { "S", "SE", "HSE", "R-Dynamic SE", "R-Dynamic HSE" },
                    ["Freelander"] = new() { "SE", "HSE" },
                    ["Range Rover"] = new() { "SE", "HSE", "Autobiography", "SV" },
                    ["Range Rover Evoque"] = new() { "S", "SE", "HSE", "R-Dynamic HSE" },
                    ["Range Rover Sport"] = new() { "SE", "HSE", "HSE Dynamic", "Autobiography", "SVR" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Tesla"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Model 3"] = new() { "RWD", "Long Range", "Performance" },
                    ["Model S"] = new() { "Long Range", "Plaid" },
                    ["Model X"] = new() { "Long Range", "Plaid" },
                    ["Model Y"] = new() { "RWD", "Long Range", "Performance" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["MG"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["MG3"] = new() { "Style", "Excite" },
                    ["MG5"] = new() { "Style MT", "Excite CVT", "Elegance CVT" },
                    ["MG GT"] = new() { "Style", "Excite", "Alpha" },
                    ["ZS"] = new() { "Style", "Excite" },
                    ["ZS EV"] = new() { "Excite", "Elegance" },
                    ["HS"] = new() { "Excite", "Trophy" },
                    ["RX5"] = new() { "Style", "Elegance" },
                    ["Marvel R"] = new() { "Luxury", "Performance" },
                },

                // ─────────────────────────────────────────────────────────────────
                ["BYD"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Dolphin"] = new() { "Dynamic", "Premium" },
                    ["Dolphin Mini"] = new() { "Dynamic", "Premium" },
                    ["Seal"] = new() { "Dynamic", "Premium", "Performance AWD" },
                    ["Song Plus"] = new() { "DM-i", "EV" },
                    ["Yuan Plus"] = new() { "GL", "GS" },
                    ["Tang"] = new() { "EV AWD" },
                    ["Han"] = new() { "EV", "EV AWD" },
                },

                // ─────────────────────────────────────────────────────────────────
                ["Chirey"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Tiggo 2 Pro"] = new() { "Comfort", "Luxury" },
                    ["Tiggo 4 Pro"] = new() { "Comfort", "Luxury" },
                    ["Tiggo 7 Pro"] = new() { "Luxury", "Premium" },
                    ["Tiggo 8 Pro"] = new() { "Luxury", "Premium", "e+" },
                },

                // ─────────────────────────────────────────────────────────────────
                ["Omoda"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["O5"] = new() { "Life", "Unlimited" },
                    ["C5"] = new() { "Life", "Unlimited" },
                },

                // ─────────────────────────────────────────────────────────────────
                ["JAC"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["J7"] = new() { "Quantum", "Limited" },
                    ["J4"] = new() { "Comfort", "Smart" },
                    ["Sei2"] = new() { "Connect", "Limited" },
                    ["Sei3"] = new() { "Quantum", "Limited" },
                    ["Sei4 Pro"] = new() { "Connect", "Limited" },
                    ["Sei7 Pro"] = new() { "Quantum", "Limited" },
                    ["E10X"] = new() { "Cargo", "Passenger" },
                    ["E J7"] = new() { "Quantum" },
                    ["Frison T6"] = new() { "Flex", "Diesel" },
                    ["Frison T8"] = new() { "Diesel 4x4" },
                },

                // ─────────────────────────────────────────────────────────────────
                ["GWM"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Haval H6"] = new() { "Premium", "HEV" },
                    ["Haval Jolion"] = new() { "Premium", "Luxury" },
                    ["Tank 300"] = new() { "Luxury", "Premium" },
                    ["Ora 03"] = new() { "400 km", "500 km" },
                },

                // ─────────────────────────────────────────────────────────────────
                ["BAIC"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["X35"] = new() { "Fashion", "Luxury" },
                    ["X55"] = new() { "Fashion", "Luxury" },
                    ["BJ40"] = new() { "Champion", "Plus" },
                    ["EU5"] = new() { "EV" },
                },

                // ─────────────────────────────────────────────────────────────────
                ["Changan"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Alsvin"] = new() { "MT", "AT" },
                    ["CS35 Plus"] = new() { "Comfort", "Luxury" },
                    ["CS55 Plus"] = new() { "Luxury", "Premium" },
                    ["CS75 Pro"] = new() { "Luxury" },
                    ["UNI-T"] = new() { "Elite", "Limited" },
                    ["UNI-K"] = new() { "Elite", "Limited" },
                },

                // ─────────────────────────────────────────────────────────────────
                ["Jetour"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["X70"] = new() { "Comfort", "Luxury" },
                    ["X70 Plus"] = new() { "Luxury", "Premium" },
                    ["Dashing"] = new() { "Comfort", "Luxury" },
                    ["T2"] = new() { "Luxury", "Premium" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Lincoln"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Corsair"] = new() { "Premier FWD", "Reserve AWD", "Grand Touring PHEV" },
                    ["Nautilus"] = new() { "Premiere", "Reserve", "Black Label" },
                    ["Aviator"] = new() { "Reserve", "Black Label", "Grand Touring" },
                    ["Navigator"] = new() { "Premiere", "Reserve", "Black Label", "Navigator L Reserve", "Navigator L Black Label" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Smart"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Fortwo"] = new() { "Passion", "Pro", "Prime" },
                    ["Forfour"] = new() { "Passion", "Pro", "Prime" },
                    ["#1"] = new() { "Standard", "Premium", "Performance" },
                    ["#6"] = new() { "Base PHEV", "Premium PHEV" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Geely"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["Coolray"] = new() { "1.5T GL", "1.5T GF", "1.5T GF Bitono" },
                    ["Emgrand"] = new() { "GL", "GLX", "Luxury" },
                    ["Cityray"] = new() { "Standard", "Luxury" },
                    ["Starray"] = new() { "Standard", "Premium", "Performance" },
                    ["EX2"] = new() { "Electric Standard", "Electric Premium" },
                    ["EX5"] = new() { "EM-i GL", "EM-i Premium", "EV Standard" },
                    ["Monjaro"] = new() { "Standard", "Luxury", "Premium" },
                    ["Okavango"] = new() { "Standard", "Luxury" },
                    ["GX3 Pro"] = new() { "Standard", "Luxury" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Acura"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["ILX"] = new() { "Standard", "A-Spec", "Tech" },
                    ["TLX"] = new() { "Standard", "A-Spec", "SH-AWD" },
                    ["RDX"] = new() { "Standard", "A-Spec", "Advance", "Type S" },
                    ["MDX"] = new() { "Standard", "A-Spec", "Advance", "Type S" },
                    ["ZDX"] = new() { "Base", "A-Spec", "Tech" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Infiniti"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["QX50"] = new() { "Pure", "Luxe", "Sensory" },
                    ["QX55"] = new() { "Pure", "Luxe", "Sensory" },
                    ["QX60"] = new() { "Pure", "Luxe", "Sensory" },
                    ["QX80"] = new() { "Sensory 4WD", "Autograph 4WD" },
                },
                // ─────────────────────────────────────────────────────────────────
                ["Isuzu"] = new(StringComparer.OrdinalIgnoreCase)
                {
                    ["D-Max"] = new() { "Workhorse", "Space Cab", "Worker", "V-Cross", "Highlander", "X-Terrain" },
                    ["MU-X"] = new() { "Comfort", "Premium", "RS", "Ultimate" },
                },

            };


        /// <summary>Lista de todas las marcas ordenada alfabéticamente.</summary>
        public static List<string> ObtenerMarcas() =>
            _catalogo.Keys.OrderBy(m => m).ToList();

        /// <summary>Lista de modelos para una marca dada. Vacía si la marca no existe.</summary>
        public static List<string> ObtenerModelos(string marca)
        {
            if (string.IsNullOrWhiteSpace(marca)) return new();
            return _catalogo.TryGetValue(marca, out var modelos)
                ? modelos.Keys.OrderBy(m => m).ToList()
                : new();
        }

        /// <summary>Lista de versiones para una marca + modelo. Vacía si no existe.</summary>
        public static List<string> ObtenerVersiones(string marca, string modelo)
        {
            if (string.IsNullOrWhiteSpace(marca) || string.IsNullOrWhiteSpace(modelo)) return new();
            if (!_catalogo.TryGetValue(marca, out var modelos)) return new();
            return modelos.TryGetValue(modelo, out var versiones) ? versiones : new();
        }

        /// <summary>True si la marca existe en el catálogo.</summary>
        public static bool ExisteMarca(string marca) =>
            !string.IsNullOrWhiteSpace(marca) && _catalogo.ContainsKey(marca);

        /// <summary>True si el modelo existe para esa marca.</summary>
        public static bool ExisteModelo(string marca, string modelo) =>
            ExisteMarca(marca) && _catalogo[marca].ContainsKey(modelo);
    }
}