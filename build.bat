@echo off

echo Building VelcroPhysics...
cd VelcroPhysics\src\VelcroPhysics
dotnet restore "VelcroPhysics.csproj"
dotnet clean "VelcroPhysics.csproj"
dotnet build "VelcroPhysics.csproj"
cd ..
cd ..
cd ..

echo Building SDL2-CS...
cd "SDL2-CS"
dotnet restore "SDL2-CS.Core.csproj"
dotnet clean "SDL2-CS.Core.csproj"
dotnet build "SDL2-CS.Core.csproj"
cd ..

echo Building GEngine-R...
dotnet restore "GEngine-R.csproj"
dotnet clean "GEngine-R.csproj"
dotnet build --no-restore "GEngine-R.csproj"

echo Done!
pause