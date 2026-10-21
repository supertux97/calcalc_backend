namespace calcalc.Models;

public class FoodItemsSyncEvent
{
    public int Id { get; set; }
    public string EventName { get; set; }
    public DateTime Time { get; set; }
    public bool Successfull { get; set; }
}