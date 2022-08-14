using dd_andromeda_poisson_disk_sampling.Propereties;

namespace dd_andromeda_poisson_disk_sampling.Services
{
    public interface IPointSettings
    {
        float Margin { get; set; }
        
        float GetPointRadius(int currentTry);
        
        int GetSearchSize(float pointRadius, in GridProperties gridProperties);
    }
}