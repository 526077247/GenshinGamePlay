using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TaoTie
{


    public class SaveTextureData:IDisposable
    {
        public int texIndex = -1;
        public int referenceCount = 0;
        public Rect rect;
        public Sprite sprite;
        public static SaveTextureData Create()
        {
            return ObjectPool.Instance.Fetch(typeof (SaveTextureData)) as SaveTextureData;
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(sprite);
            ObjectPool.Instance.Recycle(this);
        }
    }

    public class GetTextureData:IDisposable
    {
        public string name;
        public static GetTextureData Create()
        {
            return ObjectPool.Instance.Fetch(typeof (GetTextureData)) as GetTextureData;
        }

        public void Dispose()
        {
            ObjectPool.Instance.Recycle(this);
        }
    }

    public class IntegerRectangle:IDisposable
    {
        public int x;
        public int y;
        public int width;
        public int height;

        public int right => x + width;
        public int top => y + height;
        public int size => width * height;

        public Rect rect => new Rect(x, y, width, height);

        private void OnCreate(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        public static IntegerRectangle Create(int x, int y, int width, int height)
        {
            var res = ObjectPool.Instance.Fetch(typeof (IntegerRectangle)) as IntegerRectangle;
            res.OnCreate(x, y, width, height);
            return res;
        }

        public void Dispose()
        {
            ObjectPool.Instance.Recycle(this);
        }
        public override string ToString()
        {
            return string.Format("x{0}_y:{1}_width:{2}_height{3}_top:{4}_right{5}", x, y, width, height, top, right);
        }
    }
}