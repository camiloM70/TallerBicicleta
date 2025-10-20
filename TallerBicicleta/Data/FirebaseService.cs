using Google.Cloud.Firestore;

namespace TallerBicicleta.Data
{
    public class FirebaseService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirebaseService(IConfiguration configuration)
        {
            var projectId = configuration["Firebase:ProjectId"];
            var credentialsPath = configuration["Firebase:CredentialsPath"];

            if (string.IsNullOrEmpty(projectId))
                throw new InvalidOperationException("Firebase ProjectId no está configurado en appsettings.json");

            if (!string.IsNullOrEmpty(credentialsPath))
            {
                if (!File.Exists(credentialsPath))
                    throw new FileNotFoundException($"El archivo de credenciales de Firebase no existe: {credentialsPath}");

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
            }

            _firestoreDb = FirestoreDb.Create(projectId);
        }

        public FirestoreDb GetFirestoreDb() => _firestoreDb;

        public CollectionReference GetCollection(string collectionName) => _firestoreDb.Collection(collectionName);
    }
}
