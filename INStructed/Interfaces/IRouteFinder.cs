// IRouteFinder.cs
using System.Collections.Generic;

namespace INStructed.Interfaces
{
    public interface IRouteFinder
    {
        /// <summary>
        /// ������� ���������� ���� ����� ����� �������.
        /// </summary>
        /// <param name="start">��������� �����.</param>
        /// <param name="end">�������� �����.</param>
        /// <returns>������ �����, �������������� ���������� ����, ��� null ���� ���� ���.</returns>
        List<string> FindShortestPath(string start, string end);
    }
}