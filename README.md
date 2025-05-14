# Project Structure
    
    ```plaintext
    /Pages
      /Disks                  # Razor Pages implementation
        Index.cshtml          # List view
        Index.cshtml.cs       # List logic
        Create.cshtml         # Create form
        Create.cshtml.cs      # Create logic
        Edit.cshtml           # Edit form
        Edit.cshtml.cs        # Edit logic
      /Blazor
        /Disks
          Index.razor         # Direct service implementation
          ApiIndex.razor      # API implementation
    /Controllers
      DiskController.cs       # API endpoints
    /Services
      IDiskService.cs         # Service interface
      DiskService.cs          # Service implementation