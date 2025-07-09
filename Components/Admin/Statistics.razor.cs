using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using PRN222_Restaurant.Services.IService;

namespace PRN222_Restaurant.Components.Admin
{
    public partial class Statistics : ComponentBase, IAsyncDisposable
    {
        private StatisticsData? statistics;
        private List<TopProductStats>? topProducts;
        private List<CustomerReview>? recentReviews;
        private decimal[]? monthlySalesData;
        private Dictionary<string, decimal>? categorySalesData;
        private bool isLoading = true;
        private HubConnection? hubConnection;

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
            await InitializeSignalRConnection();
        }

        protected override async Task OnParametersSetAsync()
        {
            // This runs when navigating to the page
            if (!isLoading && monthlySalesData != null && categorySalesData != null)
            {
                // Delay to ensure DOM is ready
                await Task.Delay(200);
                await RenderChartsWithRetry();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Wait for data to be loaded and DOM to be ready
                await Task.Delay(500);

                if (!isLoading && monthlySalesData != null && categorySalesData != null)
                {
                    await RenderChartsWithRetry();
                }
            }
        }

        private async Task LoadData()
        {
            try
            {
                isLoading = true;
                StateHasChanged();

                // Load all statistics data
                var statisticsTask = StatisticsService.GetStatisticsAsync();
                var topProductsTask = StatisticsService.GetTopProductsAsync(5);
                var recentReviewsTask = StatisticsService.GetRecentReviewsAsync(4);
                var monthlySalesTask = StatisticsService.GetMonthlySalesDataAsync();
                var categorySalesTask = StatisticsService.GetCategorySalesDataAsync();

                await Task.WhenAll(statisticsTask, topProductsTask, recentReviewsTask, monthlySalesTask, categorySalesTask);

                statistics = await statisticsTask;
                topProducts = await topProductsTask;
                recentReviews = await recentReviewsTask;
                monthlySalesData = await monthlySalesTask;
                categorySalesData = await categorySalesTask;
            }
            catch (Exception ex)
            {
                // Error loading statistics - silent handling
                // Handle error - could show a toast notification
            }
            finally
            {
                isLoading = false;
                StateHasChanged();

                // Trigger chart rendering after loading is complete and DOM is updated
                if (monthlySalesData != null && categorySalesData != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(300); // Wait for DOM to update
                        await InvokeAsync(async () => await RenderChartsWithRetry());
                    });
                }
            }
        }

        private async Task RefreshData()
        {
            await LoadData();
            await Task.Delay(200); // Give time for DOM to update
            await RenderChartsWithRetry();
        }

        private async Task RenderChartsWithRetry(int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (monthlySalesData != null && categorySalesData != null)
                    {
                        await RenderCharts();
                        return; // Success, exit retry loop
                    }
                }
                catch (Exception ex)
                {
                    // Chart rendering attempt failed - silent handling
                    if (i < maxRetries - 1)
                    {
                        await Task.Delay(500 * (i + 1)); // Exponential backoff
                    }
                }
            }
            // Failed to render charts after all retries - silent handling
        }



        private async Task RenderCharts()
        {
            try
            {


                // Check if DOM elements exist
                var monthlyChartExists = await JSRuntime.InvokeAsync<bool>("eval", "document.getElementById('monthlySalesChart') !== null");
                var categoryChartExists = await JSRuntime.InvokeAsync<bool>("eval", "document.getElementById('categorySalesChart') !== null");



                if (!monthlyChartExists || !categoryChartExists)
                {

                    await Task.Delay(500);
                    return;
                }

                // Render Monthly Sales Chart
                if (monthlySalesData != null && monthlySalesData.Length > 0)
                {
                    var monthlySalesOptions = new
                    {
                        series = new[]
                        {
                            new
                            {
                                name = "Doanh thu",
                                data = monthlySalesData
                            }
                        },
                        chart = new
                        {
                            height = 350,
                            type = "area",
                            toolbar = new { show = false }
                        },
                        dataLabels = new { enabled = false },
                        stroke = new { curve = "smooth" },
                        xaxis = new
                        {
                            categories = new[] { "Th1", "Th2", "Th3", "Th4", "Th5", "Th6", "Th7", "Th8", "Th9", "Th10", "Th11", "Th12" }
                        },
                        tooltip = new
                        {
                            y = new
                            {
                                formatter = "function(val) { return val.toString().replace(/\\B(?=(\\d{3})+(?!\\d))/g, ',') + ' VNĐ'; }"
                            }
                        },
                        colors = new[] { "#3B82F6" }
                    };

                    await JSRuntime.InvokeVoidAsync("renderApexChart", "monthlySalesChart", monthlySalesOptions);

                }

                // Render Category Sales Chart
                if (categorySalesData != null && categorySalesData.Count > 0)
                {
                    var categoryLabels = categorySalesData.Keys.ToArray();
                    var categoryValues = categorySalesData.Values.ToArray();



                    var categorySalesOptions = new
                    {
                        series = categoryValues,
                        chart = new
                        {
                            height = 350,
                            type = "pie"
                        },
                        labels = categoryLabels,
                        colors = new[] { "#3B82F6", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6" },
                        legend = new
                        {
                            position = "bottom"
                        },
                        tooltip = new
                        {
                            y = new
                            {
                                formatter = "function(val) { return val.toString().replace(/\\B(?=(\\d{3})+(?!\\d))/g, ',') + 'đ'; }"
                            }
                        },
                        responsive = new[]
                        {
                            new
                            {
                                breakpoint = 480,
                                options = new
                                {
                                    chart = new { width = 200 },
                                    legend = new { position = "bottom" }
                                }
                            }
                        }
                    };

                    await JSRuntime.InvokeVoidAsync("renderApexChart", "categorySalesChart", categorySalesOptions);

                }
                else
                {
                    // No category data available for chart - silent handling
                }
            }
            catch (Exception ex)
            {
                // Error rendering charts - silent handling
            }
        }

        private async Task InitializeSignalRConnection()
        {
            try
            {
                hubConnection = new HubConnectionBuilder()
                    .WithUrl("/chatHub")
                    .Build();

                // Handle statistics updates
                hubConnection.On<StatisticsData>("StatisticsUpdated", async (updatedStats) =>
                {
                    statistics = updatedStats;
                    await InvokeAsync(StateHasChanged);
                });

                // Handle new orders
                hubConnection.On<object>("NewOrder", async (orderData) =>
                {
                    // Optionally show a toast notification
                    await InvokeAsync(StateHasChanged);
                });

                // Handle completed orders
                hubConnection.On<object>("OrderCompleted", async (orderData) =>
                {
                    // Refresh data when orders are completed
                    await LoadData();
                    if (monthlySalesData != null && categorySalesData != null)
                    {
                        await RenderCharts();
                    }
                });

                // Handle new customers
                hubConnection.On<object>("NewCustomer", async (customerData) =>
                {
                    await InvokeAsync(StateHasChanged);
                });

                await hubConnection.StartAsync();

                // Join the statistics group
                await hubConnection.SendAsync("JoinStatisticsGroup");
            }
            catch (Exception ex)
            {
                // Error initializing SignalR connection - silent handling
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (hubConnection is not null)
            {
                try
                {
                    await hubConnection.SendAsync("LeaveStatisticsGroup");
                    await hubConnection.DisposeAsync();
                }
                catch (Exception ex)
                {
                    // Error disposing SignalR connection - silent handling
                }
            }
        }
    }
}
