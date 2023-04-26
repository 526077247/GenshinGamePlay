using System.Collections.Generic;

namespace TaoTie
{
    public struct AICurve
    {
        public List<AIPoint> Data;

        public AICurve(AIPoint[] inputData)
        {
            Data = new List<AIPoint>();
            if (inputData != null)
            {
                Data.AddRange(inputData);
            }

            SortByX();
        }

        public bool FindY(float xVal, ref float yVal)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i].X > xVal)
                {
                    yVal = Data[i].Y;
                    return true;
                }
            }
            return false;
        }

        public void SortByX()
        {
            AIPoint temp;
            for (int i = 0; i < Data.Count; i++)
            {
                for (int j = 0; j < Data.Count - 1; j++)
                {
                    if (Data[i].X < Data[j].X)
                    {
                        temp = Data[i];
                        Data[i] = Data[j];
                        Data[j] = temp;
                    }
                }
            }
        }
    }
}