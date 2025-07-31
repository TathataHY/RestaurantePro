using System.Globalization;
using System.Resources;
using System.Text.Json;

namespace RestaurantePro.Mobile.Services
{
    public class LocalizationService
    {
        private static LocalizationService _instance;
        private static readonly object _lock = new object();
        
        public static LocalizationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new LocalizationService();
                    }
                }
                return _instance;
            }
        }

        private Dictionary<string, Dictionary<string, string>> _translations;
        private CultureInfo _currentCulture;
        private readonly string _defaultLanguage = "es";

        public event EventHandler CultureChanged;

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    CultureChanged?.Invoke(this, EventArgs.Empty);
                    SaveLanguagePreference();
                }
            }
        }

        public List<CultureInfo> SupportedCultures { get; private set; }

        private LocalizationService()
        {
            InitializeTranslations();
            LoadLanguagePreference();
        }

        private void InitializeTranslations()
        {
            _translations = new Dictionary<string, Dictionary<string, string>>
            {
                ["es"] = new Dictionary<string, string>
                {
                    // Navegación
                    ["Home"] = "Inicio",
                    ["Orders"] = "Pedidos",
                    ["Products"] = "Productos",
                    ["Preparation"] = "Preparación",
                    ["Ingredients"] = "Ingredientes",
                    ["Reservations"] = "Reservaciones",
                    ["Invoices"] = "Facturas",
                    ["Customers"] = "Clientes",
                    ["Settings"] = "Configuración",

                    // Configuración
                    ["Appearance"] = "Apariencia",
                    ["Theme"] = "Tema",
                    ["Light"] = "Claro",
                    ["Dark"] = "Oscuro",
                    ["System"] = "Sistema",
                    ["Notifications"] = "Notificaciones",
                    ["Performance"] = "Rendimiento",
                    ["Data"] = "Datos",
                    ["Language"] = "Idioma",
                    ["Accessibility"] = "Accesibilidad",

                    // Acciones
                    ["Save"] = "Guardar",
                    ["Cancel"] = "Cancelar",
                    ["Delete"] = "Eliminar",
                    ["Edit"] = "Editar",
                    ["Add"] = "Agregar",
                    ["Search"] = "Buscar",
                    ["Filter"] = "Filtrar",
                    ["Sort"] = "Ordenar",
                    ["Refresh"] = "Actualizar",
                    ["Close"] = "Cerrar",

                    // Estados
                    ["Loading"] = "Cargando...",
                    ["NoData"] = "No hay datos",
                    ["Error"] = "Error",
                    ["Success"] = "Éxito",
                    ["Warning"] = "Advertencia",
                    ["Info"] = "Información",

                    // Comandas
                    ["OrderNumber"] = "Número de Pedido",
                    ["OrderDate"] = "Fecha del Pedido",
                    ["OrderStatus"] = "Estado del Pedido",
                    ["OrderTotal"] = "Total del Pedido",
                    ["Pending"] = "Pendiente",
                    ["InProgress"] = "En Progreso",
                    ["Completed"] = "Completado",
                    ["Cancelled"] = "Cancelado",

                    // Productos
                    ["ProductName"] = "Nombre del Producto",
                    ["ProductPrice"] = "Precio del Producto",
                    ["ProductCategory"] = "Categoría del Producto",
                    ["ProductDescription"] = "Descripción del Producto",
                    ["ProductStock"] = "Stock del Producto",

                    // Clientes
                    ["CustomerName"] = "Nombre del Cliente",
                    ["CustomerEmail"] = "Email del Cliente",
                    ["CustomerPhone"] = "Teléfono del Cliente",
                    ["CustomerAddress"] = "Dirección del Cliente",

                    // Facturas
                    ["InvoiceNumber"] = "Número de Factura",
                    ["InvoiceDate"] = "Fecha de Factura",
                    ["InvoiceAmount"] = "Monto de Factura",
                    ["InvoiceStatus"] = "Estado de Factura",
                    ["Paid"] = "Pagado",
                    ["Unpaid"] = "No Pagado",
                    ["Overdue"] = "Vencido",

                    // Reservaciones
                    ["ReservationDate"] = "Fecha de Reservación",
                    ["ReservationTime"] = "Hora de Reservación",
                    ["ReservationGuests"] = "Número de Invitados",
                    ["ReservationStatus"] = "Estado de Reservación",
                    ["Confirmed"] = "Confirmada",
                    ["Pending"] = "Pendiente",
                    ["Cancelled"] = "Cancelada"
                },
                ["en"] = new Dictionary<string, string>
                {
                    // Navigation
                    ["Home"] = "Home",
                    ["Orders"] = "Orders",
                    ["Products"] = "Products",
                    ["Preparation"] = "Preparation",
                    ["Ingredients"] = "Ingredients",
                    ["Reservations"] = "Reservations",
                    ["Invoices"] = "Invoices",
                    ["Customers"] = "Customers",
                    ["Settings"] = "Settings",

                    // Settings
                    ["Appearance"] = "Appearance",
                    ["Theme"] = "Theme",
                    ["Light"] = "Light",
                    ["Dark"] = "Dark",
                    ["System"] = "System",
                    ["Notifications"] = "Notifications",
                    ["Performance"] = "Performance",
                    ["Data"] = "Data",
                    ["Language"] = "Language",
                    ["Accessibility"] = "Accessibility",

                    // Actions
                    ["Save"] = "Save",
                    ["Cancel"] = "Cancel",
                    ["Delete"] = "Delete",
                    ["Edit"] = "Edit",
                    ["Add"] = "Add",
                    ["Search"] = "Search",
                    ["Filter"] = "Filter",
                    ["Sort"] = "Sort",
                    ["Refresh"] = "Refresh",
                    ["Close"] = "Close",

                    // States
                    ["Loading"] = "Loading...",
                    ["NoData"] = "No data",
                    ["Error"] = "Error",
                    ["Success"] = "Success",
                    ["Warning"] = "Warning",
                    ["Info"] = "Information",

                    // Orders
                    ["OrderNumber"] = "Order Number",
                    ["OrderDate"] = "Order Date",
                    ["OrderStatus"] = "Order Status",
                    ["OrderTotal"] = "Order Total",
                    ["Pending"] = "Pending",
                    ["InProgress"] = "In Progress",
                    ["Completed"] = "Completed",
                    ["Cancelled"] = "Cancelled",

                    // Products
                    ["ProductName"] = "Product Name",
                    ["ProductPrice"] = "Product Price",
                    ["ProductCategory"] = "Product Category",
                    ["ProductDescription"] = "Product Description",
                    ["ProductStock"] = "Product Stock",

                    // Customers
                    ["CustomerName"] = "Customer Name",
                    ["CustomerEmail"] = "Customer Email",
                    ["CustomerPhone"] = "Customer Phone",
                    ["CustomerAddress"] = "Customer Address",

                    // Invoices
                    ["InvoiceNumber"] = "Invoice Number",
                    ["InvoiceDate"] = "Invoice Date",
                    ["InvoiceAmount"] = "Invoice Amount",
                    ["InvoiceStatus"] = "Invoice Status",
                    ["Paid"] = "Paid",
                    ["Unpaid"] = "Unpaid",
                    ["Overdue"] = "Overdue",

                    // Reservations
                    ["ReservationDate"] = "Reservation Date",
                    ["ReservationTime"] = "Reservation Time",
                    ["ReservationGuests"] = "Number of Guests",
                    ["ReservationStatus"] = "Reservation Status",
                    ["Confirmed"] = "Confirmed",
                    ["Pending"] = "Pending",
                    ["Cancelled"] = "Cancelled"
                },
                ["fr"] = new Dictionary<string, string>
                {
                    // Navigation
                    ["Home"] = "Accueil",
                    ["Orders"] = "Commandes",
                    ["Products"] = "Produits",
                    ["Preparation"] = "Préparation",
                    ["Ingredients"] = "Ingrédients",
                    ["Reservations"] = "Réservations",
                    ["Invoices"] = "Factures",
                    ["Customers"] = "Clients",
                    ["Settings"] = "Paramètres",

                    // Settings
                    ["Appearance"] = "Apparence",
                    ["Theme"] = "Thème",
                    ["Light"] = "Clair",
                    ["Dark"] = "Sombre",
                    ["System"] = "Système",
                    ["Notifications"] = "Notifications",
                    ["Performance"] = "Performance",
                    ["Data"] = "Données",
                    ["Language"] = "Langue",
                    ["Accessibility"] = "Accessibilité",

                    // Actions
                    ["Save"] = "Enregistrer",
                    ["Cancel"] = "Annuler",
                    ["Delete"] = "Supprimer",
                    ["Edit"] = "Modifier",
                    ["Add"] = "Ajouter",
                    ["Search"] = "Rechercher",
                    ["Filter"] = "Filtrer",
                    ["Sort"] = "Trier",
                    ["Refresh"] = "Actualiser",
                    ["Close"] = "Fermer",

                    // States
                    ["Loading"] = "Chargement...",
                    ["NoData"] = "Aucune donnée",
                    ["Error"] = "Erreur",
                    ["Success"] = "Succès",
                    ["Warning"] = "Avertissement",
                    ["Info"] = "Information",

                    // Orders
                    ["OrderNumber"] = "Numéro de Commande",
                    ["OrderDate"] = "Date de Commande",
                    ["OrderStatus"] = "Statut de Commande",
                    ["OrderTotal"] = "Total de Commande",
                    ["Pending"] = "En Attente",
                    ["InProgress"] = "En Cours",
                    ["Completed"] = "Terminé",
                    ["Cancelled"] = "Annulé",

                    // Products
                    ["ProductName"] = "Nom du Produit",
                    ["ProductPrice"] = "Prix du Produit",
                    ["ProductCategory"] = "Catégorie du Produit",
                    ["ProductDescription"] = "Description du Produit",
                    ["ProductStock"] = "Stock du Produit",

                    // Customers
                    ["CustomerName"] = "Nom du Client",
                    ["CustomerEmail"] = "Email du Client",
                    ["CustomerPhone"] = "Téléphone du Client",
                    ["CustomerAddress"] = "Adresse du Client",

                    // Invoices
                    ["InvoiceNumber"] = "Numéro de Facture",
                    ["InvoiceDate"] = "Date de Facture",
                    ["InvoiceAmount"] = "Montant de Facture",
                    ["InvoiceStatus"] = "Statut de Facture",
                    ["Paid"] = "Payé",
                    ["Unpaid"] = "Non Payé",
                    ["Overdue"] = "En Retard",

                    // Reservations
                    ["ReservationDate"] = "Date de Réservation",
                    ["ReservationTime"] = "Heure de Réservation",
                    ["ReservationGuests"] = "Nombre d'Invités",
                    ["ReservationStatus"] = "Statut de Réservation",
                    ["Confirmed"] = "Confirmée",
                    ["Pending"] = "En Attente",
                    ["Cancelled"] = "Annulée"
                }
            };

            SupportedCultures = new List<CultureInfo>
            {
                new CultureInfo("es"),
                new CultureInfo("en"),
                new CultureInfo("fr")
            };
        }

        public string GetString(string key)
        {
            var language = _currentCulture?.TwoLetterISOLanguageName ?? _defaultLanguage;
            
            if (_translations.ContainsKey(language) && _translations[language].ContainsKey(key))
            {
                return _translations[language][key];
            }

            // Fallback to default language
            if (_translations.ContainsKey(_defaultLanguage) && _translations[_defaultLanguage].ContainsKey(key))
            {
                return _translations[_defaultLanguage][key];
            }

            return key; // Return key if translation not found
        }

        public string GetString(string key, params object[] args)
        {
            var format = GetString(key);
            return string.Format(format, args);
        }

        private void LoadLanguagePreference()
        {
            try
            {
                var savedLanguage = Preferences.Get("SelectedLanguage", _defaultLanguage);
                _currentCulture = new CultureInfo(savedLanguage);
            }
            catch
            {
                _currentCulture = new CultureInfo(_defaultLanguage);
            }
        }

        private void SaveLanguagePreference()
        {
            Preferences.Set("SelectedLanguage", _currentCulture.TwoLetterISOLanguageName);
        }

        public async Task SetLanguageAsync(string languageCode)
        {
            try
            {
                var culture = new CultureInfo(languageCode);
                CurrentCulture = culture;
                
                // Update system culture
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting language: {ex.Message}");
            }
        }

        public string GetCurrentLanguageName()
        {
            return _currentCulture?.NativeName ?? "Español";
        }

        public List<string> GetSupportedLanguageNames()
        {
            return SupportedCultures.Select(c => c.NativeName).ToList();
        }
    }
} 