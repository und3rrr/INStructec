namespace INStructed.Interfaces
{
    public interface IMapRenderer
    {
        void Render(Graphics g, List<Point> route, Dictionary<string, IRoom> rooms);
    }
}