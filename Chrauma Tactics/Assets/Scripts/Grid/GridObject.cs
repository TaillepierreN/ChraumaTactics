using System.Collections.Generic;
using CT.Grid;
using UnityEngine;

namespace CT.Grid
{

    public class GridObject
    {
        private GridSystem _gridSystem;
        private GridPosition _gridPosition;
        //private List<Squad> _squadList;

        public GridObject(GridSystem gridSystem, GridPosition gridPosition)
        {
            _gridSystem = gridSystem;
            _gridPosition = gridPosition;
            //_squadList = new List<Squad>();
        }

        public GridPosition GetGridPosition()
        {
            return _gridPosition;
        }
        /*public void AddSquad(Squad squad)
        {
            _squadList.Add(squad);
        }*/

        /*public void RemoveSquad(Squad squad)
        {
            _squadList.Remove(squad);
        }*/

        /*public List<Squad> GetSquadList()
        {
            return _squadList;
        }*/

        public override string ToString()
        {
            string squadString = "";
            ///foreach (Squad squad in _squadList)
            ///    squadString += squad + "\n";
            return _gridPosition.ToString() + "\n" + squadString;

        }

        /*public bool HasAnySquad()
        {
            return _squadList.Count > 0;
        }*/
    }

}