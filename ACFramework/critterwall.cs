// For AC Framework 1.2, default parameters were added

using System;
using System.Drawing;
using System.Windows.Forms;

namespace ACFramework
{

    class cCritterWall : cCritter
    {
        public const float THICKNESS = 0.2f;
        public static readonly float CORNERJIGGLETURN = (float)Math.PI / 4; /* To keep someone from impossibly bouncing up and down on a corner,
			 we jiggle the bounces off corners by a small random amount.*/
        public static readonly float CORNERJIGGLEKICK = 1.15f;
        public static readonly Color WALLFILLCOLOR = Color.LightGray;
        public const float WALLPRISMDZ = 0.75f; //Default z-depth to use for drawing cCritterWall.
        protected Color _defaultfillcolor;
        protected cRealBox3 _pskeleton;

        protected int outcode(cVector3 globalpos)
        {
            return outcodeLocal(globalToLocalPosition(globalpos));
        }


        protected int outcodeLocal(cVector3 localpos)
        { 		/* This tells you which of the 27 possible positions a 
			localpos has relative to the pskeleton box */
            return _pskeleton.outcode(localpos);
        }

        public cCritterWall()
            : base(null)
        {
            _pskeleton = null;
            _defaultfillcolor = WALLFILLCOLOR;
            initialize(new cVector3(-0.5f, 0.0f), new cVector3(0.0f, 0.5f),
                THICKNESS, WALLPRISMDZ, null);
        }

        public cCritterWall(cVector3 enda)
            : base(null)
        {
            _pskeleton = null;
            _defaultfillcolor = WALLFILLCOLOR;
            initialize(enda, new cVector3(0.0f, 0.5f),
                THICKNESS, WALLPRISMDZ, null);
        }

        public cCritterWall(cVector3 enda, cVector3 endb)
            : base(null)
        {
            _pskeleton = null;
            _defaultfillcolor = WALLFILLCOLOR;
            initialize(enda, endb, THICKNESS, WALLPRISMDZ, null);
        }

        public cCritterWall(cVector3 enda, cVector3 endb, float thickness = THICKNESS,
            float height = WALLPRISMDZ, cGame pownergame = null)
            : base(pownergame)
        {
            _pskeleton = null;
            _defaultfillcolor = WALLFILLCOLOR;
            initialize(enda, endb, thickness, height, pownergame);
        }


        public cCritterWall(cVector3 enda, cVector3 endb, float thickness, cGame pownergame)
            : base(pownergame)
        {
            _pskeleton = null;
            _defaultfillcolor = WALLFILLCOLOR;
            initialize(enda, endb, thickness, WALLPRISMDZ, pownergame);
        }


        public void initialize(cVector3 enda, cVector3 endb, float thickness, float height, cGame pownergame)
        {
            _defaultprismdz = height; //Used if cSprite is a cPolygon 
            FixedFlag = true; /* By default a wall is fixed;
			remember this if you want to use one for a paddle. */
            _collidepriority = cCollider.CP_WALL; /* Don't use
			the setCollidePriority mutator, as that
			forces a call to pgame()->buildCollider(); */
            _wrapflag = cCritter.WRAP; /* In case a wall extends
			across the _border, don't bounce it. Note that
			we have overloaded setWrap so you can't turn
			off the WRAP */
            setEndsThicknessHeight(enda, endb,
                thickness, _defaultprismdz);
        }


        public override void copy(cCritter pcritter)
        {
            /*We need to overload the cCritter copy because if I have 
        cCritterWallRobot which is a cCritterWall, and it gets split in two 
        by a replicate call, then I want the new copy to have all the same 
        shooting behavior as the old one.  In general, I should
        overload copy for each of my critter child classes, but for now this 
        one is the most important.  The overload does the regular copy, then 
        looks if the thing being copied is a cCritterWall, and if it is then 
        it copies the additional fields. */
            base.copy(pcritter);
            if (!pcritter.IsKindOf("cCritterWall"))
                return; //You're done if pcritter isn't a cCritterWall*.
            cCritterWall pcritterwall = (cCritterWall)(pcritter);
            /* I know it is a cCritterWall at this point, but I need
        to do a cast, so the compiler will let me
        access cCritterWall methods and fields. */
            cRealBox3 r = new cRealBox3();
            r.copy(pcritterwall.Skeleton);
            Skeleton = r;
            _defaultfillcolor = pcritterwall._defaultfillcolor;
        }

        public override cCritter copy()
        {
            cCritterWall c = new cCritterWall();
            c.copy(this);
            return c;
        }

        public override bool IsKindOf(string str)
        {
            return str == "cCritterWall" || base.IsKindOf(str);
        }

        //Mutators 

        public void setEndsThicknessHeight(cVector3 enda, cVector3 endb,
            float thickness = THICKNESS, float height = WALLPRISMDZ)
        {
            _position = enda.add(endb).mult(0.5f);
            _wrapposition1.copy(_position);
            _wrapposition2.copy(_position);
            _wrapposition3.copy(_position);
            /* This line is important, as otherwise the 
        cCritter.draw will thing this thing was wrapped,
        and it'll get drawn in two places. */
            _tangent = endb.sub(enda);
            float length = _tangent.Magnitude;
            _tangent.normalize();
            _oldtangent.copy(_tangent);
            _normal = _tangent.defaultNormal(); /* We orient so that
			the normal is oriented to the tangent as the "y-axis"
			is to the the "x-axis".*/
            _binormal = _tangent.mult(_normal);
            _attitude = new cMatrix3(_tangent, _normal, _binormal, _position);
            Skeleton = new cRealBox3(length, thickness, height);
            Speed = 0.0f; /* Also sets _velocity to ZEROVECTOR,
			but doesn't wipe out _direction. */
            /*In looking at these settings, think of the wall as aligned horizontally with endb - enda pointing to the right and the normal pointing into the screen*/
            cPolygon ppolygon = new cPolygon(4);
            ppolygon.Edged = true;
            ppolygon.FillColor = Color.Gray;
            ppolygon.LineWidthWeight = cColorStyle.LW_IGNORELINEWIDTHWEIGHT;
            ppolygon.LineWidth = 1;
            //Means draw a one-pixel edge line.
            ppolygon.setVertex(0, new cVector3(0.5f * length, 0.5f * thickness));
            ppolygon.setVertex(1, new cVector3(-0.5f * length, 0.5f * thickness));
            ppolygon.setVertex(2, new cVector3(-0.5f * length, -0.5f * thickness));
            ppolygon.setVertex(3, new cVector3(0.5f * length, -0.5f * thickness));
            ppolygon.fixCenterAndRadius(); /* Use this call after a bunch
			of setVertex if points are just where you want. */
            ppolygon.SpriteAttitude = cMatrix3.translation(new cVector3(0.0f, 0.0f, -height / 2.0f));
            /* This corrects for the fact that we always draw the ppolygon with its
        bottom face in the xy plane and its top in the plane z = height.  We
        shift it down so it's drawn to match the skeleton positon. */
            Sprite = ppolygon; /* Also sets cSprite._prismdz to
			cCritter._defaultprismdz, which we set to 
			CritterWall.WALLPRISMDZ in our cCritterWall 
			constructor. */
        }


        public void setHeight(float newheight = 0.0f)
        {
            Skeleton = new cRealBox3(Length, Thickness, newheight);
            Sprite.PrismDz = newheight;
            Sprite.SpriteAttitude = cMatrix3.translation(new cVector3(0.0f, 0.0f, -Height / 2.0f));
            /* This corrects for the fact that we always draw the ppolygon with its
        bottom face in the xy plane and its top in the plane z = height.  We
        shift it down so it's drawn to match the skeleton positon. */
        }


        public override void mutate(int mutationflags, float mutationstrength) /* This is exactly the same as the 
			base class cCritter::mutate except that we comment out the line, which would mutate the  
			sprite. We don't want to mutate a wall's sprite. */
        { /* This is exactly the same as the base class cCritter.mutate except that we comment out the last line, which would mutate the sprite.  We don't want to mutate a wall's sprite. */
            if ((mutationflags & MF_NUDGE) != 0)  //Special kind of weak mutation
            {
                float turnangle = Framework.randomOb.randomSign()
                    * Framework.randomOb.randomReal((float)-Math.PI / 2, (float)Math.PI / 2);
                _velocity.turn(turnangle);
                _velocity.multassign(
                    Framework.randomOb.randomReal(0.5f, 1.5f));
                randomizePosition(RealBox);
            }
            if ((mutationflags & MF_POSITION) != 0)
                randomizePosition(_movebox);
            if ((mutationflags & MF_VELOCITY) != 0)
                randomizeVelocity(cCritter.MINSPEED, _maxspeedstandard);
        }

        //Accessors 

        //Serialize methods 
        //Overloads 

        public override int dragTo(cVector3 newposition, float dt)
        {
            if (!draggable())
                return cRealBox3.BOX_INSIDE; //Don't change the velocity.
            /* I'm going to allow for the possibility that I have a 3D creature in
        a 2D game world, as when I put a cSpriteQuake into a board game like 
        DamBuilder.  When I drag the walls, I still want them to be positioned
        so their butts are sitting on the xy plane. I'll run the in3DWorld test
        on the pgame->border().zsize as opposed to on the _movebox.zsize(). */
            _position.copy(newposition);
            _wrapposition1.copy(_position);
            _wrapposition2.copy(_position);
            _wrapposition3.copy(_position);
            return clamp(_dragbox);
        }

        /* Overload this so as not to change
            velocity as I normally want my walls to be stable and not drift after being dragged. */

        public override bool collide(cCritter pcritter)
        {
            cVector3 oldlocalpos, newlocalpos;
            float newdistance;
            int oldoutcode, newoutcode;
            bool crossedwall;

            oldlocalpos = globalToLocalPosition(pcritter.OldPosition);
            oldoutcode = _pskeleton.outcode(oldlocalpos);
            newlocalpos = globalToLocalPosition(pcritter.Position);
            newdistance = _pskeleton.distanceToOutcode(newlocalpos,
                out newoutcode); //Sets the newoutcode as well.
            crossedwall = crossed(oldoutcode, newoutcode);

            if (newdistance >= pcritter.Radius && !crossedwall) //No collision 
                return false; /*See if there's a collision at all. We
		say there's a collision if crossedwall or if the
		cCritterWall.distance is less than radius.  Remember that
		cCritterWall.distance measures the distance to the OUTSIDE 
		PERIMETER of the box, not the distance to the box's center. */

            /* I collided, so I need to move back further into the last good
            zone I was in outside the wall.  I want to set newlocalpos so 
            the rim of its critter is touching the wall. The idea is to back
            up in the direction of oldlocalpos.  To allow the possibility
            of skidding along the wall, we plan to back up from the
            the face (or edge or corner) facing oldlocalpos.  This works
            only if oldlocalpos was a good one, not inside the box.  In 
            principle this should always be true, but some rare weird circumstance
            (like a triple collsion) might mess this up, so we check for the
            bad case before starting. */

            if (oldoutcode == cRealBox3.BOX_INSIDE) //Note that this almost never happens.
            {
                cVector3 insidepos = new cVector3();
                insidepos.copy(oldlocalpos);
                oldlocalpos.subassign(pcritter.Tangent.mult(_pskeleton.MaxSize));
                //Do a brutally large backup to get out of the box for sure.
                oldoutcode = _pskeleton.outcode(oldlocalpos);
                //Recalculate outcode at this new position.
                oldlocalpos = _pskeleton.closestSurfacePoint(oldlocalpos, oldoutcode,
                    insidepos, cRealBox3.BOX_INSIDE, false);
                //Go to the closest surface point from there.
                oldoutcode = _pskeleton.outcode(oldlocalpos);
                //Recalculate outcode one more time to be safe.
                crossedwall = crossed(oldoutcode, newoutcode);
                //Recalculate crossedwall 
            }
            /* I find that with this code, the mouse can drag things through walls,
        so I do a kludge to block it by setting crossedwall to TRUE, this
        affects the action of cRealBox.closestSurfacePoint, as modified
        in build 34_4. */
            if (pcritter.Listener.IsKindOf("cListenerCursor"))
                crossedwall = true; //Don't trust the mouse listener.
            newlocalpos = _pskeleton.closestSurfacePoint(oldlocalpos, oldoutcode,
                newlocalpos, newoutcode, crossedwall);
            /* This call to closestSurfacePoint will move the newlocal pos
        from the far new side (or inside, or overlapping) of the box back to 
        the surface, usually on the old near side, edge, or corner given by
        oldoutcode. This prevents going through the	wall.
            If oldoutcode is a corner position and you are in fact heading
        towards a face near the corner, we used to bounce off the corner
        even though visually you can see you should bounce off the
        face.  This had the effect of making a scooter player get hung up on
        a corner sometimes. As of build 34_3, I'm moving the 
        newlocalpos to the newoutocode side in the case where oldlocalpos
        is an edge or a corner, and where crossedwall isn't TRUE.  I
        have to force in a TRUE for the cCursorLIstener case.  The USEJIGGLE
        code below also helps keep non-player critters from getting stuck
        on corners. */
            //Now back away from the box.
            newoutcode = _pskeleton.outcode(newlocalpos);
            cVector3 avoidbox = _pskeleton.escapeVector(newlocalpos, newoutcode);
            newlocalpos.addassign(avoidbox.mult(pcritter.Radius));
            newoutcode = _pskeleton.outcode(newlocalpos);
            pcritter.moveTo(localToGlobalPosition(newlocalpos), true);
            //TRUE means continuous motion, means adjust tangent etc.
            //Done with position, now change the velocity 
            cVector3 localvelocity = globalToLocalDirection(pcritter.Velocity);
            cVector3 oldlocalvelocity = new cVector3();
            oldlocalvelocity.copy(localvelocity);
            _pskeleton.reflect(localvelocity, newoutcode);
            /* I rewrote the reflect code on Feb 22, 2004 for VErsion 34_3, changing
        it so that when you reflect off an edge or corner, you only bounce
        the smallest of your three velocity components. Balls stll seem to
        get hung up on the corner once is awhile. */
            /* Now decide, depending on the pcritter's absorberflag and bounciness,
        how much you want to use the new localvelocity vs. the 
        oldlocalvelocity. We decompose the new localvelocity into the
        tangentvelocity parallel to the wall and the normalvelocity
        away from the wall. Some pencil and paper drawings convince
        me that the tangent is half the sum of the oldlocalvelocity
        and the reflected new localvelocity. */
            cVector3 tangentvelocity = localvelocity.add(oldlocalvelocity).mult(0.5f);
            cVector3 normalvelocity = localvelocity.sub(tangentvelocity);
            float bouncefactor = 1.0f;
            if (pcritter.AbsorberFlag)
                bouncefactor = 0.0f;
            else
                bouncefactor = pcritter.Bounciness;
            localvelocity = tangentvelocity.add(normalvelocity.mult(bouncefactor));
            /* Maybe the rotation should depend on the kind of edge or corner.
            Right now let's just use critter's binormal. Don't to it 
            to the player or viewer as it's confusing.  */
            if (!(cRealBox3.isFaceOutcode(newoutcode)) && //edge or corner 
                !(pcritter.IsKindOf("cCritterViewer")) && //not viewer 
                !(pcritter.IsKindOf("cCritterArmedPlayer")))
            //Not player.  Note that cPlayer inherits from cCritterArmedPlayer, 
            //so don't use cCritterPlayer as the base class here.
            {
                localvelocity.rotate(new cSpin(
                    Framework.randomOb.randomReal(
                        -cCritterWall.CORNERJIGGLETURN,
                        cCritterWall.CORNERJIGGLETURN), //A random turn 
                        pcritter.Binormal)); //Around the critter's binormal 
                localvelocity.multassign(cCritterWall.CORNERJIGGLEKICK); //Goose it a little 
            }
            pcritter.Velocity = localToGlobalDirection(localvelocity);
            return true;
        }


        public override int collidesWith(cCritter pcritterother)
        {
            /* Make sure I don't ever waste time colliding walls with
    walls. I only call this the one time that I enroll the cCritterWall
     into the cGame's _pcollider. */
            if (pcritterother.IsKindOf("cCritterWall"))
                return cCollider.DONTCOLLIDE;
            return base.collidesWith(pcritterother);
        }

        /* Overload to rule out possibliity of 
            all/wall collision,	even if they aren't fixed. */

        public new float distanceTo(cVector3 vpoint)
        {
            return _pskeleton.distanceTo(globalToLocalPosition(vpoint));
        }


        public override int clamp(cRealBox3 border)
        { /* We don't change _pskeleton as it has the geometric info.  We 
		just change _position. */
            if (_baseAccessControl == 1)
                return base.clamp(border);
            cRealBox3 effectivebox = border;
            cVector3 oldcorner;
            cVector3 newcorner = new cVector3();
            int outcode = 0;
            int totaloutcode = 0;
            for (int i = 0; i < 8; i++) //Step through the wall's corners 
            {
                oldcorner = _pskeleton.corner(i).add(_position);
                newcorner.copy(oldcorner);
                outcode = effectivebox.clamp(newcorner);
                if (outcode != cRealBox3.BOX_INSIDE) //corner was moved 
                {
                    _position.addassign(newcorner.sub(oldcorner));
                    /* As long at the wall is small enough to 
                fit inside the border, the successive 
                corrections won't cancel each other out. */
                    totaloutcode |= outcode;
                }
            }
            _wrapposition1.copy(_position);
            _wrapposition2.copy(_position);
            _wrapposition3.copy(_position);
            /* So it won't think it wrapped. */
            return outcode;
        }


        public override string RuntimeClass
        {
            get
            {
                return "cCritterWall";
            }
        }

        public virtual float Thickness
        {
            get
                { return _pskeleton.YSize; }
            set
            {
                Skeleton = new cRealBox3(Length, value, Height);
                cPolygon ppolygon = (cPolygon) Sprite;
                ppolygon.setVertex(0, new cVector3(0.5f * Length, 0.5f * Thickness));
                ppolygon.setVertex(1, new cVector3(-0.5f * Length, 0.5f * Thickness));
                ppolygon.setVertex(2, new cVector3(-0.5f * Length, -0.5f * Thickness));
                ppolygon.setVertex(3, new cVector3(0.5f * Length, -0.5f * Thickness));
                ppolygon.fixCenterAndRadius(); /* Use this call after a bunch
			    of setVertex if points are just where you want. */
            }
        }

        public virtual cRealBox3 Skeleton
        {
            get
            { return _pskeleton; }
            set
            {
                _pskeleton = value;
            }
        }

        public virtual Color FillColor
        {
            set
            {
                _defaultfillcolor = value;
                if (_psprite != null)
                    _psprite.FillColor = value;
            }
        }

        public virtual float XRadius
        {
            get
                { return _pskeleton.XRadius; }
        }

        public virtual float YRadius
        {
            get
                { return _pskeleton.YRadius; }
        }

        public virtual float ZRadius
        {
            get
                { return _pskeleton.ZRadius; }
        }

        public virtual float Length
        {
            get
                { return _pskeleton.XSize; }
        }

        public virtual float Height
        {
            get
                { return _pskeleton.ZSize; }
        }

        public override int WrapFlag
        {
            set
            {
                if (_baseAccessControl == 1)
                    base.WrapFlag = value;
            }
        }
        //Don't allow _wrapflag to change from cCritter::WRAP.
        //Special method 

        public bool crossed(int startoutcode, int endoutcode)
        {
            /* If crossed is TRUE then moving from start to end may
        mean you moved across the wall, even though neither start 
        nor end has to be close to the wall.  The only way to get a false
        positive is if you move very rapidly from, like LOY to HIX, skipping
        over the corner zone.  If you have a largish radius and smallish
        speed this shouldn't happen.  Our checks work by noticing when you
        leave a side zone.  To check against moving into the block exactly from
        a corner we include the BOX_INSIDE checks as well. */

            return
                (startoutcode == cRealBox3.BOX_LOX && ((endoutcode & cRealBox3.BOX_LOX) == 0)) ||
                (startoutcode == cRealBox3.BOX_HIX && ((endoutcode & cRealBox3.BOX_HIX) == 0)) ||
                (startoutcode == cRealBox3.BOX_LOY && ((endoutcode & cRealBox3.BOX_LOY) == 0)) ||
                (startoutcode == cRealBox3.BOX_HIY && ((endoutcode & cRealBox3.BOX_HIY) == 0)) ||
                (startoutcode == cRealBox3.BOX_LOZ && ((endoutcode & cRealBox3.BOX_LOZ) == 0)) ||
                (startoutcode == cRealBox3.BOX_HIZ && ((endoutcode & cRealBox3.BOX_HIZ) == 0)) ||
                startoutcode == cRealBox3.BOX_INSIDE || endoutcode == cRealBox3.BOX_INSIDE; //For corners 
        }


        public virtual bool blocks(cVector3 start, cVector3 end)
        {
            return crossed(outcode(start), outcode(end));
        }

        /* Returns 
            TRUE if the wall blocks a line drawn from start to end. */

    }

    //This is still underconstruction
    //TODO: Add nessesary data memebers, finsish update function
    class cCritterWallGate : cCritterWall
    {
        protected bool open = false;
        protected cVector3 _startPosition;
        protected cVector3 _moveToPosition;
        protected float _moveSpeed;

        public cCritterWallGate(cVector3 moveToPosition, float moveSpeed, 
            cVector3 enda, cVector3 endb, float thickness = THICKNESS,
            float height = WALLPRISMDZ, cGame pownergame = null)
            : base(enda,endb,thickness,height,pownergame)
        {
            _pskeleton = null;
            _defaultfillcolor = WALLFILLCOLOR;
            initialize(enda, endb, thickness, height, pownergame);
            _startPosition = _position;
            _moveToPosition = moveToPosition;
            _moveSpeed = moveSpeed;
        }

        public virtual bool Open
        {
            get { return open; }
            set { open = value; }
        }
        
        public override bool IsKindOf(string str)
        {
            return str == "cCritterWallGate" || base.IsKindOf(str);
        }
        
        public override string RuntimeClass
        {
            get
            {
                return "cCritterWallGate";
            }
        }
        


        public void moveGate(cVector3 startPosition, cVector3 moveToPosition, float moveSpeed)
        {
            if (open == false && this.Position != startPosition)
            {
                moveTo(startPosition);
                dragTo(startPosition, moveSpeed);
            }
            else if (open == true && this.Position != moveToPosition)
            {
                moveTo(moveToPosition);
                dragTo(moveToPosition, moveSpeed);
            }
        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            moveGate(_startPosition, _moveToPosition, _moveSpeed);
        }
    }
}


                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              