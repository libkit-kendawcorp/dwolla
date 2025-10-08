using System.Text;
using System.Net.Http.Headers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using JsonException
= System.Text.Json.JsonException;

namespace DwollaSDK
{
    public static class Dwolla
    {
        /// <summary>
        /// Retrieve the API root entry point to discover available resources and endpoints based on your OAuth access token permissions.
        /// Returns HAL+JSON with navigation links to accessible resources including accounts, customers, events, and webhook subscriptions depending on token scope. 
        /// Essential for API exploration, dynamic resource discovery, and building adaptive client applications that respond to available permissions.
        /// </summary>
        public static string ApiBaseUrl { get; private set; }
        = "https://api-sandbox.dwolla.com";

        private static bool _isSandboxMode;

        public static bool IsSandboxMode
        {
            get => _isSandboxMode;
            set
            {
                _isSandboxMode
                = value;

                switch (value)
                {
                    case true:
                        {
                            ApiBaseUrl
                            = "https://api-sandbox.dwolla.com";
                        }
                        break;
                    case false:
                        {
                            ApiBaseUrl
                            = "https://api.dwolla.com";
                        }
                        break;
                }
            }
        }

        public static class Auth
        {
            /// <summary>
            /// OAuth get token request. Client credentials are sent in the Authorization header using Basic authentication.
            /// </summary>
            public static string Creds { get; set; }
                = string.Empty;

            /// <summary>
            /// A new access token that is used to authenticate against resources that belong to the app itself.
            /// </summary>
            public static string Token { get; private set; }
                = string.Empty;

            /// <summary>
            /// The lifetime of the access token, in seconds. Default is 3600.
            /// </summary>
            public static TimeSpan Expires { get; private set; }
                = new TimeSpan();

            /// <summary>
            /// Generate an application access token using OAuth 2.0 client credentials flow for server-to-server authentication. 
            /// Requires client ID and secret sent via Basic authentication header with grant_type=client_credentials in the request body. 
            /// Returns a bearer access token with expiration time for authenticating API requests scoped to your application. 
            /// Essential for secure API access.
            /// </summary>
            public static async Task GetAccessTokenAsync()
            {
                using (HttpClient HttpClient = new HttpClient())
                {
                    byte[] Bytes
                    = System.Text
                    .Encoding.UTF8
                    .GetBytes(Creds);

                    string String
                    = Convert
                    .ToBase64String(Bytes);

                    HttpClient.DefaultRequestHeaders
                    .Authorization
                    = new AuthenticationHeaderValue(
                    "Basic",
                    String);

                    FormUrlEncodedContent FormUrlEncodedContent
                    = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>
                        ("grant_type",
                        "client_credentials")
                    });

                    HttpResponseMessage HttpResponseMessage
                    = await HttpClient
                    .PostAsync(
                    ApiBaseUrl
                    + "/token",
                    FormUrlEncodedContent);

                    string Response
                    = await HttpResponseMessage
                    .Content
                    .ReadAsStringAsync();

                    bool Boolean
                    = !HttpResponseMessage
                    .IsSuccessStatusCode;

                    switch (Boolean)
                    {
                        case true:
                            {
                                throw new HttpRequestException(
                                $"Dwolla token request failed with status {(int)HttpResponseMessage.StatusCode} {HttpResponseMessage.ReasonPhrase}: {Response}");
                            }
                    }

                    try
                    {
                        JToken JToken
                        = JToken.Parse(Response)
                        ?? throw new InvalidOperationException
                        ("Failed to deserialize Dwolla token response.");

                        Token
                        = (string)JToken["access_token"];

                        Expires
                        = TimeSpan.FromSeconds
                        ((int)JToken["expires_in"]);
                    }
                    catch (JsonException JsonException)
                    {
                        throw new InvalidOperationException(
                        "Invalid JSON format in Dwolla response.",
                        JsonException);
                    }
                }
            }
        }

        /// <summary>
        // Creates a new customer with different verification levels and capabilities. 
        // Supports personal verified customers (individuals), business verified customers (businesses), unverified customers, and receive-only users. 
        // Customer type determines transaction limits, verification requirements, and available features.
        /// </summary>
        public class Customer
        {
            public string? ID
            { get; private set; }

            public string? FirstName
            { get; private set; }

            public string? LastName
            { get; private set; }

            public string? Email
            { get; private set; }

            public CustomerType? Type
            { get; private set; }

            public CustomerStatus? Status
            { get; private set; }

            public DateTime? Created
            { get; private set; }

            internal Customer(
            JToken JToken)
            {
                ID
                = (string)
                JToken["id"];

                FirstName
                = (string)
                JToken["firstName"];

                LastName
                = (string)
                JToken["lastName"];

                Email
                = (string)
                JToken["email"];

                {
                    switch (
                    (string)
                    JToken["type"])
                    {
                        case "unverified":
                            Type
                            = CustomerType
                            .Unverified;
                            break;

                        case "receive-only":
                            Type
                            = CustomerType
                            .ReceiveOnly;
                            break;

                        case "personal":
                            Type
                            = CustomerType
                            .Personal;
                            break;

                        case "soleProp":
                            Type
                            = CustomerType
                            .SoleProp;
                            break;

                        case "business":
                            Type
                            = CustomerType
                            .Business;
                            break;

                        case "business-with-controller":
                            Type
                            = CustomerType
                            .BusinessWithController;
                            break;

                        case "business-with-international-controller":
                            Type
                            = CustomerType
                            .BusinessWithInternationalController;
                            break;

                        default:
                            Type
                            = CustomerType
                            .Unknown;
                            break;
                    }
                }

                {
                    switch (
                   (string)
                   JToken["status"])
                    {
                        case "unverified":
                            Status
                            = CustomerStatus
                            .Unverified;
                            break;

                        case "retry":
                            Status
                            = CustomerStatus
                           .Retry;
                            break;

                        case "document":
                            Status
                            = CustomerStatus
                           .Document;
                            break;

                        case "verified":
                            Status
                            = CustomerStatus
                           .Verified;
                            break;

                        case "suspended":
                            Status
                            = CustomerStatus
                           .Suspended;
                            break;

                        case "deactivated":
                            Status
                            = CustomerStatus
                           .Deactivated;
                            break;

                        default:
                            Status
                            = CustomerStatus
                           .Unknown;
                            break;
                    }
                }

                Created
                = (DateTime)
                JToken["created"];
            }

            public enum CustomerType
            {
                Unknown,
                Unverified,
                ReceiveOnly,
                Personal,
                SoleProp,
                Business,
                BusinessWithController,
                BusinessWithInternationalController
            }

            public enum CustomerStatus
            {
                Unknown,
                Unverified,
                Retry,
                Document,
                Verified,
                Suspended,
                Deactivated
            }

            /// <summary>
            // Returns a paginated list of customers sorted by creation date. 
            // Supports fuzzy search across customer names, 
            // business names, and email addresses, plus exact filtering by email and verification status. 
            // Default limit is 25 customers per page, maximum 200.
            /// </summary>
            /// <param name="Offset">How many results to skip</param>
            /// <param name="Limit">How many results to return</param>
            /// <param name="Search">Searches on certain fields</param>
            /// <param name="Status">Filter by customer status</param>
            public static async Task<Customer[]> GetCustomersAsync(
            int Offset = 0,
            int Limit = 30,
            string Search = "",
            CustomerStatus Status
            = CustomerStatus.Unknown
            )
            {
                using (HttpClient HttpClient
                = new HttpClient())
                {
                    bool
                    Boolean;

                    HttpClient.DefaultRequestHeaders
                    .Accept.Add(
                    new MediaTypeWithQualityHeaderValue
                    ("application/vnd.dwolla.v1.hal+json"));

                    HttpClient.DefaultRequestHeaders
                    .Authorization
                    = new AuthenticationHeaderValue(
                    "Bearer",
                    Auth.Token);

                    {
                        Boolean
                        = Limit
                        > 200;

                        switch (Boolean)
                        {
                            case true:
                                Limit
                                = 200;
                                break;
                        }
                    }

                    StringBuilder StringBuilder
                    = new StringBuilder();

                    StringBuilder
                    .Append(ApiBaseUrl);
                    StringBuilder
                    .Append("/customers?");

                    StringBuilder
                    .Append(
                    "offset="
                    + Offset);

                    StringBuilder
                    .Append(
                    "&limit="
                    + Limit);

                    {
                        Boolean
                        = Search
                        != String.Empty;

                        switch (Boolean)
                        {
                            case true:
                                {
                                    StringBuilder
                                    .Append(
                                    "&search="
                                    + System.Uri
                                    .EscapeDataString(Search));
                                }
                                break;
                        }
                    }

                    {
                        Boolean
                        = Status
                        != CustomerStatus
                        .Unknown;

                        switch (Boolean)
                        {
                            case true:
                                {
                                    StringBuilder
                                    .Append(
                                    "&status="
                                    + Status
                                    .ToString()
                                    .ToLower());
                                }
                                break;
                        }
                    }

                    string Uri
                    = StringBuilder
                    .ToString();

                    HttpResponseMessage HttpResponseMessage
                    = await HttpClient
                    .GetAsync(Uri);

                    string Response
                    = await HttpResponseMessage
                    .Content
                    .ReadAsStringAsync();

                    Boolean
                    = !HttpResponseMessage
                    .IsSuccessStatusCode;

                    switch (Boolean)
                    {
                        case true:
                            {
                                throw new HttpRequestException(
                                $"Dwolla GET /customers failed: {(int)HttpResponseMessage.StatusCode} {HttpResponseMessage.ReasonPhrase}\n{Response}");
                            }
                    }

                    try
                    {
                        JToken JToken
                        = JToken.Parse(Response)
                        ?? throw new InvalidOperationException
                        ("Failed to deserialize Dwolla token response.");

                        List<Customer> List
                        = new List<Customer>();

                        JArray JArray
                        = JToken
                        ["_embedded"]
                        ["customers"]
                        .Value<JArray>();

                        foreach (JToken _JToken
                        in JArray)
                        {
                            Customer Customer
                            = new Customer(_JToken);

                            List
                            .Add(Customer);
                        }

                        Array Array
                        = List
                        .ToArray();

                        return
                        (Customer[])Array;
                    }
                    catch (JsonException JsonException)
                    {
                        throw new InvalidOperationException
                        ("Dwolla returned an empty response body.");
                    }
                }
            }

            public static async Task<bool> CreateReceiveOnlyUserAsync(
            string FirstName,
            string LastName,
            string Email
            )
            {
                using (HttpClient HttpClient = new HttpClient())
                {
                    HttpClient.DefaultRequestHeaders
                    .Accept.Add(
                    new MediaTypeWithQualityHeaderValue
                    ("application/vnd.dwolla.v1.hal+json"));

                    HttpClient.DefaultRequestHeaders
                    .Authorization
                    = new AuthenticationHeaderValue(
                    "Bearer",
                    Auth.Token);

                    StringContent StringContent
                    = new StringContent(
                    new JObject
                    {
                        ["firstName"]
                        = FirstName,

                        ["lastName"]
                        = LastName,

                        ["email"]
                        = Email,

                        ["type"]
                        = "receive-only"
                    }.ToString
                    (Formatting.None),
                    Encoding.UTF8,
                    "application/vnd.dwolla.v1.hal+json");

                    HttpResponseMessage HttpResponseMessage
                    = await HttpClient
                    .PostAsync(
                    ApiBaseUrl
                    + "/customers",
                    StringContent);

                    string Response
                    = await HttpResponseMessage
                    .Content
                    .ReadAsStringAsync();

                    bool Boolean
                    = HttpResponseMessage
                    .IsSuccessStatusCode;

                    return
                    Boolean;
                }
            }

            public static async Task<bool> CreateUnverifiedCustomerAsync(
            string FirstName,
            string LastName,
            string Email
            )
            {
                using (HttpClient HttpClient = new HttpClient())
                {
                    HttpClient.DefaultRequestHeaders
                    .Accept.Add(
                    new MediaTypeWithQualityHeaderValue
                    ("application/vnd.dwolla.v1.hal+json"));

                    HttpClient.DefaultRequestHeaders
                    .Authorization
                    = new AuthenticationHeaderValue(
                    "Bearer",
                    Auth.Token);

                    StringContent StringContent
                    = new StringContent(
                    new JObject
                    {
                        ["firstName"]
                        = FirstName,

                        ["lastName"]
                        = LastName,

                        ["email"]
                        = Email
                    }.ToString
                    (Formatting.None),
                    Encoding.UTF8,
                    "application/vnd.dwolla.v1.hal+json");

                    HttpResponseMessage HttpResponseMessage
                    = await HttpClient
                    .PostAsync(
                    ApiBaseUrl
                    + "/customers",
                    StringContent);

                    string Response
                    = await HttpResponseMessage
                    .Content
                    .ReadAsStringAsync();

                    bool Boolean
                    = HttpResponseMessage
                    .IsSuccessStatusCode;

                    return
                    Boolean;
                }
            }

            public static async Task<bool> CreateVerifiedPersonalCustomerAsync(
            string FirstName,
            string LastName,
            string Email,
            string Address1,
            string Address2,
            string City,
            string State,
            int PostalCode,
            int SSN,
            DateTime DateOfBirth
            )
            {
                using (HttpClient HttpClient = new HttpClient())
                {
                    HttpClient.DefaultRequestHeaders
                    .Accept.Add(
                    new MediaTypeWithQualityHeaderValue
                    ("application/vnd.dwolla.v1.hal+json"));

                    HttpClient.DefaultRequestHeaders
                    .Authorization
                    = new AuthenticationHeaderValue(
                    "Bearer",
                    Auth.Token);

                    StringContent StringContent
                    = new StringContent(
                    new JObject
                    {
                        ["firstName"]
                        = FirstName,

                        ["lastName"]
                        = LastName,

                        ["email"]
                        = Email,

                        ["type"]
                        = "personal",

                        ["address1"]
                        = Address1,

                        ["address2"]
                        = Address2,

                        ["city"]
                        = City,

                        ["state"]
                        = State,

                        ["postalCode"]
                        = PostalCode,

                        ["ssn"]
                        = SSN,

                        ["dateOfBirth"]
                        = DateOfBirth
                    }.ToString
                    (Formatting.None),
                    Encoding.UTF8,
                    "application/vnd.dwolla.v1.hal+json");

                    HttpResponseMessage HttpResponseMessage
                    = await HttpClient
                    .PostAsync(
                    ApiBaseUrl
                    + "/customers",
                    StringContent);

                    string Response
                    = await HttpResponseMessage
                    .Content
                    .ReadAsStringAsync();

                    bool Boolean
                    = HttpResponseMessage
                    .IsSuccessStatusCode;

                    return
                    Boolean;
                }
            }

            // Test
            public static async Task<bool> CreateVerifiedSolePropCustomerAsync(
            string FirstName,
            string LastName,
            string Email,
            string Address1,
            string Address2,
            string City,
            string State,
            int PostalCode,
            int SSN,
            DateTime DateOfBirth,
            Business.Classification
            BusinessClassification,
            string BusinessName,
            string BusinessEIN
            )
            {
                using (HttpClient HttpClient = new HttpClient())
                {
                    HttpClient.DefaultRequestHeaders
                    .Accept.Add(
                    new MediaTypeWithQualityHeaderValue
                    ("application/vnd.dwolla.v1.hal+json"));

                    HttpClient.DefaultRequestHeaders
                    .Authorization
                    = new AuthenticationHeaderValue(
                    "Bearer",
                    Auth.Token);

                    StringContent StringContent
                    = new StringContent(
                    new JObject
                    {
                        ["firstName"]
                        = FirstName,

                        ["lastName"]
                        = LastName,

                        ["email"]
                        = Email,

                        ["type"]
                        = "business",

                        ["address1"]
                        = Address1,

                        ["address2"]
                        = Address2,

                        ["city"]
                        = City,

                        ["state"]
                        = State,

                        ["postalCode"]
                        = PostalCode,

                        ["ssn"]
                        = SSN,

                        ["dateOfBirth"]
                        = DateOfBirth,

                        ["businessClassification"]
                        = BusinessClassification
                        .ID,

                        ["businessName"]
                        = BusinessName,

                        ["ein"]
                        = BusinessEIN,

                        ["businessType"]
                        = "soleProprietorship"
                    }.ToString
                    (Formatting.None),
                    Encoding.UTF8,
                    "application/vnd.dwolla.v1.hal+json");

                    HttpResponseMessage HttpResponseMessage
                    = await HttpClient
                    .PostAsync(
                    ApiBaseUrl
                    + "/customers",
                    StringContent);

                    string Response
                    = await HttpResponseMessage
                    .Content
                    .ReadAsStringAsync();

                    bool Boolean
                    = HttpResponseMessage
                    .IsSuccessStatusCode;

                    return
                    Boolean;
                }
            }

            // Test
            public static async Task<bool> CreateVerifiedBusinessCustomerWithControllerAsync(
            string FirstName,
            string LastName,
            string Email,
            string Address1,
            string Address2,
            string City,
            string State,
            int PostalCode,
            int SSN,
            DateTime DateOfBirth,
            Business.Classification
            BusinessClassification,
            string BusinessName,
            string BusinessEIN
            )
            {
                using (HttpClient HttpClient = new HttpClient())
                {
                    HttpClient.DefaultRequestHeaders
                    .Accept.Add(
                    new MediaTypeWithQualityHeaderValue
                    ("application/vnd.dwolla.v1.hal+json"));

                    HttpClient.DefaultRequestHeaders
                    .Authorization
                    = new AuthenticationHeaderValue(
                    "Bearer",
                    Auth.Token);

                    StringContent StringContent
                    = new StringContent(
                    new JObject
                    {
                        ["firstName"]
                        = FirstName,

                        ["lastName"]
                        = LastName,

                        ["email"]
                        = Email,

                        ["type"]
                        = "business",

                        ["address1"]
                        = Address1,

                        ["address2"]
                        = Address2,

                        ["city"]
                        = City,

                        ["state"]
                        = State,

                        ["postalCode"]
                        = PostalCode,

                        ["ssn"]
                        = SSN,

                        ["dateOfBirth"]
                        = DateOfBirth,

                        ["businessClassification"]
                        = BusinessClassification
                        .ID,

                        ["businessName"]
                        = BusinessName,

                        ["ein"]
                        = BusinessEIN,

                        ["businessType"]
                        = "soleProprietorship"
                    }.ToString
                    (Formatting.None),
                    Encoding.UTF8,
                    "application/vnd.dwolla.v1.hal+json");

                    HttpResponseMessage HttpResponseMessage
                    = await HttpClient
                    .PostAsync(
                    ApiBaseUrl
                    + "/customers",
                    StringContent);

                    string Response
                    = await HttpResponseMessage
                    .Content
                    .ReadAsStringAsync();

                    bool Boolean
                    = HttpResponseMessage
                    .IsSuccessStatusCode;

                    return
                    Boolean;
                }
            }

            public class Business
            {
                /// <summary>
                // Returns a directory of business and industry classifications required for creating business verified customers. 
                // Each business classification contains multiple industry classifications. 
                // The industry classification ID must be provided in the businessClassification parameter during business customer creation for verification.
                /// </summary>
                public class Classification
                {
                    public string ID
                    { get; private set; }

                    public string Name
                    { get; private set; }

                    internal Classification(
                    JToken JToken)
                    {
                        ID
                        = (string)
                        JToken["id"];

                        Name
                        = (string)
                        JToken["name"];
                    }

                    public static async Task<Classification[]> GetClassificationsAsync()
                    {
                        using (HttpClient HttpClient
                        = new HttpClient())
                        {
                            HttpClient.DefaultRequestHeaders
                            .Accept.Add(
                            new MediaTypeWithQualityHeaderValue
                            ("application/vnd.dwolla.v1.hal+json"));

                            HttpClient.DefaultRequestHeaders
                            .Authorization
                            = new AuthenticationHeaderValue(
                            "Bearer",
                            Auth.Token);

                            StringBuilder StringBuilder
                            = new StringBuilder();

                            StringBuilder
                            .Append(ApiBaseUrl);
                            StringBuilder
                            .Append("/business-classifications");

                            string Uri
                            = StringBuilder
                            .ToString();

                            HttpResponseMessage HttpResponseMessage
                            = await HttpClient
                            .GetAsync(Uri);

                            string Response
                            = await HttpResponseMessage
                            .Content
                            .ReadAsStringAsync();

                            bool Boolean
                            = !HttpResponseMessage
                            .IsSuccessStatusCode;

                            switch (Boolean)
                            {
                                case true:
                                    {
                                        throw new HttpRequestException(
                                        $"Dwolla GET /customers failed: {(int)HttpResponseMessage.StatusCode} {HttpResponseMessage.ReasonPhrase}\n{Response}");
                                    }
                            }

                            try
                            {
                                JToken JToken
                                = JToken.Parse(Response)
                                ?? throw new InvalidOperationException
                                ("Failed to deserialize Dwolla token response.");

                                List<Classification> List
                                = new List<Classification>();

                                JArray JArray
                                = JToken
                                ["_embedded"]
                                ["business-classifications"]
                                .Value<JArray>();

                                foreach (JToken _JToken
                                in JArray)
                                {
                                    Classification Classification
                                    = new Classification(_JToken);

                                    List
                                    .Add(
                                    Classification);
                                }

                                Array Array
                                = List
                                .ToArray();

                                return
                                (Classification[])Array;
                            }
                            catch (JsonException JsonException)
                            {
                                throw new InvalidOperationException
                                ("Dwolla returned an empty response body.");
                            }
                        }
                    }
                }
            }
        }
    }
}

// Copyright © KenDawCorp. doing business as LibKit. All rights reserved.