public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        // Identity
        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(_ => 
            ConnectionMultiplexer.Connect(Configuration["Redis:ConnectionString"]));
        services.AddScoped<ICacheService, RedisCacheService>();

        // Trading Services
        services.AddScoped<ITradingService, TradingService>();
        services.AddScoped<IOrderExecutionService, FixOrderExecutionService>();
        services.AddSingleton<IMarketDataService, WebSocketMarketDataService>();
        
        // QuickFIX/n Configuration
        services.AddSingleton<IQuickFix>(provider => 
            new QuickFixClient(Configuration["FIX:ConfigFile"]));

        // WebSocket Market Data
        services.AddHostedService<MarketDataBackgroundService>();

        // API
        services.AddControllers();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}