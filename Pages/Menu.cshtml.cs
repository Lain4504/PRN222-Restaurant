using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using PRN222_Restaurant.Models;

namespace PRN222_Restaurant.Pages
{
    public class MenuModel : PageModel
    {
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public List<Category> Categories { get; set; } = new List<Category>();

        public void OnGet()
        {
            // In a real application, this would come from your database
            // For now, we'll use hardcoded data
            Categories = GetMockCategories();
            MenuItems = GetMockMenuItems();
        }

        private List<Category> GetMockCategories()
        {
            return new List<Category>
            {
                new Category { Id = 1, Name = "Starters" },
                new Category { Id = 2, Name = "Main Courses" },
                new Category { Id = 3, Name = "Sides" },
                new Category { Id = 4, Name = "Desserts" },
                new Category { Id = 5, Name = "Beverages" }
            };
        }

        private List<MenuItem> GetMockMenuItems()
        {
            return new List<MenuItem>
            {
                // Starters
                new MenuItem 
                { 
                    Id = 1, 
                    Name = "Seafood Sampler", 
                    Description = "A delightful assortment of shrimp cocktail, crab cakes, and calamari served with house-made sauces",
                    Price = 24.99M,
                    CategoryId = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1601050690597-df0568f70950?q=80&w=2071"
                },
                new MenuItem 
                { 
                    Id = 2, 
                    Name = "Fresh Oysters", 
                    Description = "Half dozen fresh oysters on ice served with mignonette sauce and lemon wedges",
                    Price = 18.99M,
                    CategoryId = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1615141982883-c7ad0e69fd62?q=80&w=1974"
                },
                new MenuItem 
                { 
                    Id = 3, 
                    Name = "Maryland Crab Cakes", 
                    Description = "Two jumbo lump crab cakes with minimal filler, pan-seared and served with remoulade sauce",
                    Price = 16.99M,
                    CategoryId = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1593247035809-eedff0962973?q=80&w=2070"
                },
                
                // Main Courses
                new MenuItem 
                { 
                    Id = 4, 
                    Name = "Seared Sea Scallops", 
                    Description = "Jumbo scallops pan-seared to golden perfection, served with cauliflower purée and microgreens",
                    Price = 32.99M,
                    CategoryId = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2?q=80&w=2070"
                },
                new MenuItem 
                { 
                    Id = 5, 
                    Name = "Grilled Atlantic Salmon", 
                    Description = "Perfectly grilled salmon fillet with lemon dill sauce, served with roasted vegetables and wild rice",
                    Price = 28.99M,
                    CategoryId = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1553557202-e8e60732b8fd?q=80&w=2070"
                },
                new MenuItem 
                { 
                    Id = 6, 
                    Name = "Lobster Linguine", 
                    Description = "Linguine pasta tossed with chunks of Maine lobster in a rich creamy sauce with a hint of saffron",
                    Price = 36.99M,
                    CategoryId = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1560963689-b5682b6440f8?q=80&w=2070"
                },
                
                // Sides
                new MenuItem 
                { 
                    Id = 7, 
                    Name = "Garlic Bread", 
                    Description = "Crusty French bread topped with our house-made garlic butter and herbs",
                    Price = 6.99M,
                    CategoryId = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1594046243098-0fceea9d451e?q=80&w=1974"
                },
                new MenuItem 
                { 
                    Id = 8, 
                    Name = "Seasonal Vegetables", 
                    Description = "Chef's selection of seasonal vegetables, lightly seasoned and roasted",
                    Price = 7.99M,
                    CategoryId = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?q=80&w=2084"
                },
                new MenuItem 
                { 
                    Id = 9, 
                    Name = "Truffle Fries", 
                    Description = "Crispy hand-cut fries tossed with truffle oil, parmesan cheese, and fresh herbs",
                    Price = 8.99M,
                    CategoryId = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1623238913973-21e186a7c8a5?q=80&w=2012"
                },
                
                // Desserts
                new MenuItem 
                { 
                    Id = 10, 
                    Name = "New York Cheesecake", 
                    Description = "Creamy classic New York style cheesecake with a graham cracker crust, topped with fresh berries",
                    Price = 9.99M,
                    CategoryId = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1551024506-0bccd828d307?q=80&w=1974"
                },
                new MenuItem 
                { 
                    Id = 11, 
                    Name = "Chocolate Lava Cake", 
                    Description = "Warm chocolate cake with a molten center, served with vanilla ice cream and fresh berries",
                    Price = 10.99M,
                    CategoryId = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1517427294546-5aa121f68e8a?q=80&w=1964"
                },
                new MenuItem 
                { 
                    Id = 12, 
                    Name = "Key Lime Pie", 
                    Description = "Refreshing key lime pie with a buttery graham cracker crust, topped with whipped cream",
                    Price = 8.99M,
                    CategoryId = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1563729784474-d77dbb933a9e?q=80&w=1974"
                },
                
                // Beverages
                new MenuItem 
                { 
                    Id = 13, 
                    Name = "Ocean Breeze Cocktail", 
                    Description = "Our signature blue cocktail with rum, blue curaçao, pineapple juice, and coconut cream",
                    Price = 12.99M,
                    CategoryId = 5,
                    ImageUrl = "https://images.unsplash.com/photo-1525373698358-041e3a460346?q=80&w=1964"
                },
                new MenuItem 
                { 
                    Id = 14, 
                    Name = "House White Wine", 
                    Description = "Glass of our house Chardonnay, perfect pairing with seafood dishes",
                    Price = 9.99M,
                    CategoryId = 5,
                    ImageUrl = "https://images.unsplash.com/photo-1563227812-0ea4c22e6cc8?q=80&w=2070"
                },
                new MenuItem 
                { 
                    Id = 15, 
                    Name = "Fresh Lemonade", 
                    Description = "Freshly squeezed lemonade with a hint of mint, served over ice",
                    Price = 4.99M,
                    CategoryId = 5,
                    ImageUrl = "https://images.unsplash.com/photo-1499638673689-79a0b5115d87?q=80&w=1964"
                }
            };
        }
    }
}
