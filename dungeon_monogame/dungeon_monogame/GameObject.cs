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
        Vector3 scale = Vector3.One;
        Quaternion rotation;
        List<GameObject> children;

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

        public void draw(Effect effect, Matrix transform)
        {
            transform = transform * Matrix.CreateTranslation(location) * Matrix.CreateScale(scale);
            chunkManager.draw(effect, transform);
            foreach (GameObject child in children)
            {
                child.draw(effect, transform);
            }
        }


        public void update(GameTime time)
        {
            updateChildren(time);
        }

        public void updateChildren(GameTime time)
        {
            foreach (GameObject child in children)
            {
                child.update(time);
            }
        }


    }
}
