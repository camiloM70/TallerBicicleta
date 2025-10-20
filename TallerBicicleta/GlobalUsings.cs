// GlobalUsings.cs - Gestión centralizada de espacios de nombres
// Este archivo define los using globales para todo el proyecto TallerBicicleta

// ===== SISTEMA BASE =====
global using System;
global using System.Collections.Generic;
global using System.ComponentModel.DataAnnotations;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using System.Security.Cryptography;
global using System.IO;

// ===== ASP.NET CORE =====
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Route = Microsoft.AspNetCore.Components.RouteAttribute;

// ===== BLAZOR ESPECÍFICO =====
global using Microsoft.Extensions.Logging;

// ===== PROYECTO ESPECÍFICO =====
global using TallerBicicleta.Components;
global using TallerBicicleta.Models;
global using TallerBicicleta.Services;
global using TallerBicicleta.Data;

// ===== OTROS PAQUETES =====
global using Google.Cloud.Firestore;
global using SixLabors.ImageSharp;
global using SixLabors.ImageSharp.PixelFormats;
global using ZXing;
global using ZXing.Common;
//global using Stripe;