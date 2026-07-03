using System.Text.Json;
namespace JetFlight.Service.Services;
public class FirebaseServiceAccount
{
    public static string GetServiceAccountJson()
    {
        var serviceAccountJson = new
        {
            type = Environment.GetEnvironmentVariable("FIREBASE_TYPE"),
            project_id = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID"),
            private_key_id = Environment.GetEnvironmentVariable("FIREBASE_PRIVATE_KEY_ID"),
            private_key = Environment.GetEnvironmentVariable("FIREBASE_PRIVATE_KEY"),
            client_email = Environment.GetEnvironmentVariable("FIREBASE_CLIENT_EMAIL"),
            client_id = Environment.GetEnvironmentVariable("FIREBASE_CLIENT_ID"),
            auth_uri = Environment.GetEnvironmentVariable("FIREBASE_AUTH_URI"),
            token_uri = Environment.GetEnvironmentVariable("FIREBASE_TOKEN_URI"),
            auth_provider_x509_cert_url = Environment.GetEnvironmentVariable("FIREBASE_AUTH_PROVIDER_CERT_URL"),
            client_x509_cert_url = Environment.GetEnvironmentVariable("FIREBASE_CLIENT_CERT_URL"),
            universe_domain = Environment.GetEnvironmentVariable("UNIVERSE_DOMAIN")
        };

        return JsonSerializer.Serialize(serviceAccountJson);
    }
}