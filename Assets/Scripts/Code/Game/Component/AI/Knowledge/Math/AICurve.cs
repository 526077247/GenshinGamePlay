using System.Collections.Generic;

namespace TaoTie
{
    public struct AICurve
    {
        public List<AIPoint> data;

        public AICurve(AIPoint[] inputData)
        {
            data = new List<AIPoint>();
            data.AddRange(inputData);
            SortByX();
        }

        public bool FindY(float xVal, ref float yVal)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].x > xVal)
                {
                    yVal = data[i].y;
                    return true;
                }
            }
            return false;
        }

        public void SortByX()
        {
            AIPoint temp;
            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data.Count - 1; j++)
                {
                    if (data[i].x < data[j].x)
                    {
                        temp = data[i];
                        data[i] = data[j];
                        data[j] = temp;
                    }
                }
            }
        }
    }
}