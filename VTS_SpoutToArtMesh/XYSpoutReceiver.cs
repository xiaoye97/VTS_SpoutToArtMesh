using Klak.Spout;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VTS_SpoutToArtMesh
{
    public class XYSpoutReceiver : MonoBehaviour
    {
        public static SpoutResources spoutResources;
        public bool HFlip, VFlip;

        public static void VFlipRenderTexture(RenderTexture target)
        {
            var temp = RenderTexture.GetTemporary(target.descriptor);
            Graphics.Blit(target, temp, new Vector2(1, -1), new Vector2(0, 1));
            Graphics.Blit(temp, target);
            RenderTexture.ReleaseTemporary(temp);
        }

        public static void HFlipRenderTexture(RenderTexture target)
        {
            var temp = RenderTexture.GetTemporary(target.descriptor);
            Graphics.Blit(target, temp, new Vector2(-1, 1), new Vector2(1, 0));
            Graphics.Blit(temp, target);
            RenderTexture.ReleaseTemporary(temp);
        }

        public static void HVFlipRenderTexture(RenderTexture target)
        {
            var temp = RenderTexture.GetTemporary(target.descriptor);
            Graphics.Blit(target, temp, new Vector2(-1, -1), new Vector2(1, 1));
            Graphics.Blit(temp, target);
            RenderTexture.ReleaseTemporary(temp);
        }

        public void FindSpoutResource()
        {
            if (spoutResources == null)
            {
                var spoutSender = GameObject.Find("Live2D Camera").GetComponent<SpoutSender>();
                spoutResources = spoutSender._resources;
            }
        }

        private void Awake()
        {
            FindSpoutResource();
            SetResources(spoutResources);
        }

        public List<Renderer> targetRenderers
        {
            get
            {
                return this._targetRenderers;
            }
            set
            {
                this._targetRenderers = value;
            }
        }

        [SerializeField]
        private List<Renderer> _targetRenderers;

        private void Update()
        {
            if (this._receiver == null)
            {
                this._receiver = new Receiver(this._sourceName);
            }
            this._receiver.Update();
            if (this._receiver.Texture == null || this._targetRenderers.Count == 0)
            {
                return;
            }
            RenderTexture renderTexture = this.PrepareBuffer();
            Blitter.BlitFromSrgb(this._resources, this._receiver.Texture, renderTexture);
            if (HFlip && VFlip)
            {
                HVFlipRenderTexture(renderTexture);
            }
            else if (HFlip)
            {
                HFlipRenderTexture(renderTexture);
            }
            else if (VFlip)
            {
                VFlipRenderTexture(renderTexture);
            }
            foreach (Renderer renderer in this._targetRenderers)
            {
                RendererOverride.SetTexture(renderer, this._targetMaterialProperty, renderTexture);
            }
        }

        #region 原版

        private void ReleaseReceiver()
        {
            Receiver receiver = this._receiver;
            if (receiver != null)
            {
                receiver.Dispose();
            }
            this._receiver = null;
        }

        private RenderTexture PrepareBuffer()
        {
            if (this._targetTexture != null)
            {
                if (this._buffer != null)
                {
                    Utility.Destroy(this._buffer);
                    this._buffer = null;
                }
                return this._targetTexture;
            }
            Texture2D texture = this._receiver.Texture;
            if (this._buffer != null && (this._buffer.width != texture.width || this._buffer.height != texture.height))
            {
                Utility.Destroy(this._buffer);
                this._buffer = null;
            }
            if (this._buffer == null)
            {
                this._buffer = new RenderTexture(texture.width, texture.height, 0);
                this._buffer.hideFlags = HideFlags.DontSave;
                this._buffer.Create();
            }
            return this._buffer;
        }

        private void OnDisable()
        {
            this.ReleaseReceiver();
        }

        private void OnDestroy()
        {
            Utility.Destroy(this._buffer);
            this._buffer = null;
        }

        public string sourceName
        {
            get
            {
                return this._sourceName;
            }
            set
            {
                this.ChangeSourceName(value);
            }
        }

        private void ChangeSourceName(string name)
        {
            if (this._sourceName == name)
            {
                return;
            }
            this._sourceName = name;
            this.ReleaseReceiver();
        }

        public RenderTexture targetTexture
        {
            get
            {
                return this._targetTexture;
            }
            set
            {
                this._targetTexture = value;
            }
        }

        public string targetMaterialProperty
        {
            get
            {
                return this._targetMaterialProperty;
            }
            set
            {
                this._targetMaterialProperty = value;
            }
        }

        public RenderTexture receivedTexture
        {
            get
            {
                if (!(this._buffer != null))
                {
                    return this._targetTexture;
                }
                return this._buffer;
            }
        }

        public void SetResources(SpoutResources resources)
        {
            this._resources = resources;
        }

        private Receiver _receiver;

        private RenderTexture _buffer;

        [SerializeField]
        private string _sourceName;

        [SerializeField]
        private RenderTexture _targetTexture;

        [SerializeField]
        private string _targetMaterialProperty;

        [SerializeField]
        [HideInInspector]
        private SpoutResources _resources;

        #endregion 原版
    }
}