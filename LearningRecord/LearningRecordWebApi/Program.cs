using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    // 设置允许接收非常大的请求体长度               
    options.MemoryBufferThreshold = 1;
    // 设置单个文件大小限制为2MB               
    options.MultipartBodyLengthLimit = 2097152;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

string uploadPath = (app.Environment.ContentRootPath + "/Upload").Replace("/", "\\"); 
if (!Directory.Exists(uploadPath)) 
{ 
    Directory.CreateDirectory(uploadPath); 
}           
// 使用自定义的静态文件目录           
app.UseStaticFiles(new StaticFileOptions
{ 
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.ContentRootPath, "upload")
     ),
    RequestPath = "/upload"
});

app.MapControllers();

app.Run();
