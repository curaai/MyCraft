using System;
using System.Linq;
using UnityEngine;

namespace Players
{
    public class DestoryBlockAnimator
    {
        private float destorySecond = 0;
        private float duration = 0;
        protected int stage;
        public int Stage
        {
            get { return stage; }
            set
            {
                stage = value;
                material.mainTexture = textures[Stage];
                if (value == 0)
                    material.color = fissureColor;
            }
        }
        public bool IsUpdateNow => destorySecond != 0;
        private Vector3Int? targetPos;


        private Material material;
        private Texture2D[] textures;
        private Color fissureColor = new Color(97 / 255f, 97 / 255f, 97 / 255f, 1);

        public DestoryBlockAnimator(Material destroyMaterial)
        {
            this.material = destroyMaterial;

            textures = Resources.LoadAll<Texture2D>("Textures/Destroy");
        }

        public void Init(float destorySecond, Vector3Int? targetPos)
        {
            duration = 0;
            stage = -1;

            this.destorySecond = destorySecond;
            this.targetPos = targetPos;
            material.color = Color.clear;
        }

        public void Reset()
        {
            Init(0, null);
        }

        public bool Update(in Vector3Int _targetPos)
        {
            duration += Time.deltaTime;
            if (destorySecond < duration)
            {
                Reset();
                return true;
            }
            if (targetPos.Value != _targetPos)
            {
                Reset();
                return false;
            }
            if (!IsUpdateNow)
                return false;

            var _stage = Convert.ToInt32(Math.Floor(duration / destorySecond * textures.Length));
            if (_stage != Stage)
                Stage = _stage;

            return false;
        }
    }
}