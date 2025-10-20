var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar Firebase
builder.Services.AddScoped<FirebaseService>();

// Configurar autenticación y autorización
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddAuthenticationCore();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthStateProvider>());

// Configurar servicios de negocio
builder.Services.AddScoped<EmpleadoService>();
builder.Services.AddScoped<ProductoService>();
builder.Services.AddScoped<ServicioService>();
builder.Services.AddScoped<SolicitudService>();
builder.Services.AddScoped<FacturaService>();
builder.Services.AddScoped<AbonoService>();
builder.Services.AddScoped<GananciaService>();

// Configurar servicios de utilidad
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<StripePaymentService>();
builder.Services.AddScoped<CodigoBarrasService>();
builder.Services.AddScoped<DataSeeder>();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configurar controladores y API
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Habilitar CORS
app.UseCors();

// Configurar el enrutamiento
app.UseRouting();

// Configurar la autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Configurar antiforgery después de autenticación/autorización
app.UseAntiforgery();

// Mapear controladores y componentes
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Inicializar datos por defecto (crear usuario admin)
using (var scope = app.Services.CreateScope())
{
    var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await dataSeeder.SeedAdminUserAsync();
}

app.Run();