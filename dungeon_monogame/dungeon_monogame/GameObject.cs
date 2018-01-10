using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_monogame
{
    class GameObject
    {
        ChunkManager chunkManager;
        private Vector3 location = new Vector3();
        protected Vector3 scale = Vector3.One;
        Quaternion rotation = Quaternion.Identity;
        protected List<GameObject> children;
        protected Color emissiveness = Color.Black;

        public GameObject()
        {
            chunkManager = new ChunkManager();
            children = new List<GameObject>();
        }

        public GameObject(ChunkManager _chunkManager, Vector3 _location, Vector3 _scale)
        {
            location = _location;

            scale = _scale;

            chunkManager = _chunkManager;
            children = new List<GameObject>();
        }

        public GameObject(ChunkManager _chunkManager, Vector3 _location, Vector3 _scale, Quaternion _rotation) : this(_chunkManager, _location, _scale)
        {
            rotation = _rotation;
        }

        public void addChild(GameObject child)
        {
            children.Add(child);
        }

        public Vector3 getLocation()
        {
            return location;
        }

        public void setLocation(Vector3 v)
        {
            location = v;
        }

        public void setEmissiveness(Color e)
        {
            emissiveness = e;
        }

        public void drawFirstPass(Effect effect, Matrix transform)
        {
            transform = getTransform(ref transform);
            chunkManager.draw(effect, transform, this.emissiveness);

            foreach (GameObject child in children)
            {
                child.drawFirstPass(effect, transform);
            }
        }

        public virtual void drawDeferredPass(Effect effect, Matrix transform, GraphicsDevice device)
        {
            foreach (GameObject child in children)
            {
                child.drawDeferredPass(effect, getTransform(ref transform), device);
            }
        }

        protected Matrix getTransform(ref Matrix transform)
        {
            return (Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(location)) * transform;
        }


        protected virtual List<Action> update()
        {
            return new List<Action>();
        }

        public List<Action> updateWithChildren()
        {
            List<Action> result = new List<Action>();
            result.AddRange(update());
            foreach (GameObject child in children)
            {
                result.AddRange(child.updateWithChildren());
            }
            return result;
        }



        internal ChunkManager getChunkSpace()
        {
            return chunkManager;
        }

        internal void recursiveRemove(GameObject obj)
        {
            if (children.Contains(obj))
            {
                children.Remove(obj);
            }
            else
            {
                foreach (GameObject c in children)
                {
                    c.recursiveRemove(obj);
                }
            }
        }

        public void setRotation(Quaternion quaternion)
        {
            rotation = quaternion;
        }

        public virtual void takeDamage(int damage) { }

    }
}
