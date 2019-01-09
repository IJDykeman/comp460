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
        private Vector3 location = new Vector3();
        public Vector3 scale = Vector3.One;
        Quaternion rotation = Quaternion.Identity;
        protected List<GameObject> children;
        protected Color emissiveness = Color.Black;
        private HashSet<ObjectTag> tags = new HashSet<ObjectTag>();
        

        public GameObject()
        {
            children = new List<GameObject>();
        }

        public GameObject(Vector3 _location, Vector3 _scale)
        {
            location = _location;

            scale = _scale;

            children = new List<GameObject>();
        }

        public GameObject(Vector3 _location, Vector3 _scale, Quaternion _rotation) : this(_location, _scale)
        {
            rotation = _rotation;
        }

        public void addTag(ObjectTag tag)
        {
            tags.Add(tag);
        }

        public bool hasTag(ObjectTag tag)
        {
            return tags.Contains(tag);
        }

        public List<GameObject> getChildrenWithTag(ObjectTag tag)
        {
            List<GameObject> result = new List<GameObject>();
            if (hasTag(tag)) {
                result.Add(this);
            }
            foreach (GameObject o in children){
                result.AddRange(o.getChildrenWithTag(tag));
            }
            return result;
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

        public virtual void drawAlternateGBufferFirstPass(Matrix transform)
        {
            transform = getTransform(ref transform);

            foreach (GameObject child in children)
            {
                child.drawAlternateGBufferFirstPass(transform);
            }
        }


        public virtual void draw2D(GraphicsDeviceManager device)
        {
            foreach (GameObject child in children)
            {
                child.draw2D(device);
            }
        }


        public virtual void drawFirstPass(Effect effect, Matrix transform, BoundingFrustum frustum, Predicate<GameObject> whetherDraw)
        {

            transform = getTransform(ref transform);

            foreach (GameObject child in children)
            {
                child.drawFirstPass(effect, transform, frustum, whetherDraw);
            }
        }

        public virtual void drawSecondPass(Effect effect, Matrix transform, GraphicsDevice device)
        {
            foreach (GameObject child in children)
            {
                child.drawSecondPass(effect, getTransform(ref transform), device);
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

        public virtual void burn() { }


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

        internal List<GameObject> recursiveGetWithinSphere(Vector3 center, float radius)
        {
            List<GameObject> results = new List<GameObject>();
            foreach (GameObject child in children){
                if ((child.getLocation() - center).Length() <= radius)
                {
                    results.Add(child);
                    results.AddRange(child.recursiveGetWithinSphere(center - this.getLocation(), radius));

                }
            }
            return results;
        }

        public void setRotation(Quaternion quaternion)
        {
            rotation = quaternion;
        }

        public virtual void takeDamage(float damage) { }

    }
}
