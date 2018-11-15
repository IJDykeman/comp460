using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dungeon_monogame
{
    class Slime : Monster
    {
        GameObject pose1;
        GameObject pose2;

        public Slime(Vector3 location)
        {
            setLocation(location);

            Assembly assembly = Assembly.GetExecutingAssembly();

            ChunkManager model1 = MagicaVoxel.ChunkManagerFromResource("dungeon_monogame.Content.voxel_models.slime.slime1.vox");
            ChunkManager model2 = MagicaVoxel.ChunkManagerFromResource("dungeon_monogame.Content.voxel_models.slime.slime2.vox");
            scale = Vector3.One * .4f;
            Vector3 offset = model1.getCenter();
            //offset = Vector3.One * 4.5f;
            
            pose1 = new GameObjectModel(model1, -offset, Vector3.One);
            pose2 = new GameObjectModel(model2, -offset, Vector3.One);
            addChild(pose1);

            this.aabb = model1.getAaabbFromModelExtents();
        }

        List<Action> explode()
        {

            List<Action> result = new List<Action>();
            if (getAgeSeconds() > 5)
            {
                //spawn particles
                for (int i = 0; i < 15; i++)
                {
                    var gooModel = MagicaVoxel.ChunkManagerFromResource("dungeon_monogame.Content.voxel_models.slime.goo.vox");
                    result.Add(new SpawnAction(new BalisticModel(Globals.randomVectorOnUnitSphere() + getLocation(), Globals.randomVectorOnUnitSphere() * 5f, .2f, gooModel)));
                }
                result.Add(new AofDamage(getLocation(), ObjectTag.Player, 5, 1));
                result.Add(new DissapearAction(this));
                result.Add(new SpawnAction(new Flash(getLocation())));
                
            }
            return result;


        }

        protected override List<Action> update()
        {
            List<Action> result = basicCreatureUpdate();
            recursiveRemove(pose1);
            recursiveRemove(pose2);
            float age = getAgeSeconds();
            float x = age % 4;
            float cycleTime = 1.0f;
            if (onFire)
            {
                cycleTime /= 3;
            }
            if (age % cycleTime > cycleTime / 2)
            {
                recursiveRemove(pose1);
                addChild(pose2);
            }
            else
            {
                recursiveRemove(pose2);
                addChild(pose1);

            }
            result.Add(new MoveTowardTaggedActorAction(this, ObjectTag.Player, 3, 25));
            Vector3 move = getTargetLocation() - getLocation();
            if (move.Length() < 4)
            {
                result.AddRange(explode());
            }
            move.Y = 0;
            float moveLength = move.Length();
            if (moveLength > 0) {
                move.Normalize();
                float angle = -(float)(Math.Atan2(move.Z, move.X));
                
                move *= Math.Min(getSpeed(), moveLength);

                Quaternion rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateRotationY(angle));//Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
                //pose1.setRotation(rotation);
                //pose2.setRotation(rotation);
                setRotation(rotation);
                this.setInstantaneousMovement(move);
            }

            return result;
        }
    }
}
