using System;
using System.Drawing;
using System.Windows.Forms;

// mod: setRoom1 doesn't repeat over and over again

namespace ACFramework
{ 
	
	class cCritterDoor : cCritterWall 
	{

	    public cCritterDoor(cVector3 enda, cVector3 endb, float thickness, float height, cGame pownergame ) 
		    : base( enda, endb, thickness, height, pownergame ) 
	    { 
	    }
		
		public override bool collide( cCritter pcritter ) 
		{ 
			bool collided = base.collide( pcritter ); 
			if ( collided && pcritter.IsKindOf( "cCritter3DPlayer" ) ) 
			{ 
				(( cGame3D ) Game ).setdoorcollision( ); 
				return true; 
			} 
			return false; 
		}
 
        public override bool IsKindOf( string str )
        {
            return str == "cCritterDoor" || base.IsKindOf( str );
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritterDoor";
            }
        }
	} 
	
	//==============Critters for the cGame3D: Player, Ball, Treasure ================ 
	
	class cCritter3DPlayer : cCritterArmedPlayer 
	{ 
        //private bool warningGiven = false;
		
        public cCritter3DPlayer( cGame pownergame ) 
            : base( pownergame ) 
		{ 
			BulletClass = new cCritter3DPlayerBulletBouncing( );
            Sprite = new cSpriteQuake(ModelsMD2.Mario);
			Sprite.SpriteAttitude = cMatrix3.scale( 2, 0.8f, 0.4f ); 
			setRadius( 0.5f ); //Default cCritter.PLAYERRADIUS is 0.4.  
			setHealth( 10 ); 
			moveTo( _movebox.LoCorner.add( new cVector3( 0.0f, 0.0f, 2.0f ))); 
			WrapFlag = cCritter.CLAMP; //Use CLAMP so you stop dead at edges.
			Armed = true; //Let's use bullets.
			MaxSpeed =  cGame3D.MAXPLAYERSPEED; 
			AbsorberFlag = true; //Keeps player from being buffeted about.
			ListenerAcceleration = 160.0f; //So Hopper can overcome gravity.  Only affects hop.
		
			Listener = new cListenerHopper( 6.0f, 4.0f ); 
            // the two arguments are walkspeed and hop strength -- JC
            
            addForce( new cForceGravity( 50.0f )); /* Uses  gravity. Default strength is 25.0.
			Gravity	will affect player using cListenerHopper. */ 
			AttitudeToMotionLock = false; //It looks nicer is you don't turn the player with motion.
			Attitude = new cMatrix3( new cVector3(0.0f, 0.0f, -1.0f), new cVector3( -1.0f, 0.0f, 0.0f ), 
                new cVector3( 0.0f, 1.0f, 0.0f ), Position);
            Sprite.ModelState = State.Idle;
		}

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt); //Always call this first
            if (Position.Z != Game.Border.Midz)
                moveToZ(Game.Border.Midz);
            /*if (!warningGiven && distanceTo(new cVector3(Game.Border.Lox, Game.Border.Loy,
                Game.Border.Midz)) < 3.0f)
            {
                warningGiven = true;
                //MessageBox.Show("About to go to Second Level (Underwater)!");
            }*/
 
        } 

        public override bool collide( cCritter pcritter ) 
		{ 
			bool playerhigherthancritter = Position.Y - Radius > pcritter.Position.Y; 
		/* If you are "higher" than the pcritter, as in jumping on it, you get a point
	       and the critter dies.  If you are lower than it, you lose health and the
	       critter also dies. To be higher, let's say your low point has to higher
	       than the critter's center. We compute playerhigherthancritter before the collide,
	       as collide can change the positions. */
            _baseAccessControl = 1;
			bool collided = base.collide( pcritter );
            _baseAccessControl = 0;
            if (!collided) 
				return false;
		/* If you're here, you collided.  We'll treat all the guys the same -- the collision
	       with a Treasure is different, but we let the Treasure contol that collision. */ 
			if ( playerhigherthancritter ) 
			{
                Framework.snd.play(Sound.Stomp); 
				addScore( 10 ); 
			} 
			else if(((cListenerHopper)Listener)._childsmode) 
			{ 
				damage( 1 );
                Framework.snd.play(Sound.StompAndBounce); 
			} 
			pcritter.die(); 
			return true; 
		}

        public override cCritterBullet shoot()
        {
            Framework.snd.play(Sound.Fireball);
            Sprite.ModelState = State.CrouchWeapon;
            return base.shoot();
        }

        public override bool IsKindOf( string str )
        {
            return str == "cCritter3DPlayer" || base.IsKindOf( str );
        }
		
        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DPlayer";
            }
        }
	} 
	
   
	class cCritter3DPlayerBullet : cCritterBullet 
	{

        public cCritter3DPlayerBullet() { }

        public override cCritterBullet Create()
            // has to be a Create function for every type of bullet -- JC
        {
            return new cCritter3DPlayerBullet();
        }
		
		public override void initialize( cCritterArmed pshooter ) 
		{ 
			base.initialize( pshooter );
            Sprite.FillColor = Color.Crimson;
            // can use setSprite here too
            setRadius(0.1f);
            _usefixedlifetime = false;
		} 

        public override bool IsKindOf( string str )
        {
            return str == "cCritter3DPlayerBullet" || base.IsKindOf( str );
        }
		
        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DPlayerBullet";
            }
        }
	} 
	
    //********************NEW CLASSES************************
    class cCritter3DPlayerBulletBouncing : cCritter3DPlayerBullet
    {
        public cCritter3DPlayerBulletBouncing()
        {

        }
        public override void initialize(cCritterArmed pshooter)
        {
            base.initialize(pshooter);
            //Sprite = new cSpriteIcon(BitmapRes.Fire);
            //Sprite.ColorStyle = new cColorStyle(true, true, Color.DarkOrange, Color.Black);
            Sprite = new cSpriteFireball();
            Sprite.FillColor = Color.FromArgb(240,100,0);
            setRadius(0.2f);
            UseFixedLifetime = true;
            FixedLifetime = 50.0f; 
            cVector3 start = pshooter.Position;
            this.addForce(new cForceGravity());
            if (((cListenerHopper)(pshooter.Listener)).FaceLeft)
            {
                this.yaw((float)(Math.PI));
                float bulletdistance1 = pshooter.GunLength * pshooter.Radius;
                float bulletdistance2 = pshooter.Radius + 1.5f * BULLETRADIUS; 
                float bulletdistance = (bulletdistance1 > bulletdistance2) ? bulletdistance1 : bulletdistance2;
                cVector3 end = start.add(pshooter.AimVector.neg().mult(bulletdistance));
                end = end.add(new cVector3(0.0f, 1.0f, 0.0f));
                moveTo(end); 
            }
            else if (((cListenerHopper)(pshooter.Listener)).FaceRight)
            {
                float bulletdistance1 = pshooter.GunLength * pshooter.Radius;
                float bulletdistance2 = pshooter.Radius + 1.5f * BULLETRADIUS; 
                float bulletdistance = (bulletdistance1 > bulletdistance2) ? bulletdistance1 : bulletdistance2;
                cVector3 end = start.add(pshooter.AimVector.mult(bulletdistance));
                end = end.add(new cVector3(0.0f, 1.0f, 0.0f));
                moveTo(end); 
            }
            else 
            {
                MessageBox.Show("Something has gone terribly wrong");
            }
        }

        public override cCritterBullet Create()
        {
            return new cCritter3DPlayerBulletBouncing();
        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            if (Position.Z != Game.Border.Midz)
                moveToZ(Game.Border.Midz);
        }

        public override bool IsKindOf(string str)
        {
            return str == "cCritter3DPlayerBulletBouncing" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DPlayerBulletBouncing";
            }
        }
    }

    //******************END NEW CLASSES***********************
	
	class cCritter3Dcharacter : cCritter  
	{ 
		
        public cCritter3Dcharacter( cGame pownergame ) 
            : base( pownergame ) 
		{ 
			addForce( new cForceGravity( 50.0f, new cVector3( 0.0f, -1, 0.00f ))); 
			addForce( new cForceDrag( 20.0f ) );  // default friction strength 0.5 
			Density = 2.0f; 
			MaxSpeed = 30.0f;
            if (pownergame != null) //Just to be safe.
                //Sprite = new cSpriteQuake(Framework.models.selectRandomCritter());
                Sprite = new cSpriteQuake(ModelsMD2.Turtle);
            
            // example of setting a specific model
            // setSprite(new cSpriteQuake(ModelsMD2.Knight));
            
            if ( Sprite.IsKindOf( "cSpriteQuake" )) //Don't let the figurines tumble.  
			{ 
				AttitudeToMotionLock = false;   
				Attitude = new cMatrix3( new cVector3( 0.0f, 0.0f, 1.0f ), 
                    new cVector3( 1.0f, 0.0f, 0.0f ), 
                    new cVector3( 0.0f, 1.0f, 0.0f ), Position); 
				/* Orient them so they are facing towards positive Z with heads towards Y. */ 
			} 
			Bounciness = 0.0f; //Not 1.0 means it loses a bit of energy with each bounce.
			setRadius( 0.5f );
            MinTwitchThresholdSpeed = 4.0f; // Means sprite doesn't switch direction unless it's moving fast 
			randomizePosition( new cRealBox3( new cVector3( _movebox.Lox + 2.0f, _movebox.Midy, _movebox.Midz), 
				new cVector3( _movebox.Hix, _movebox.Loy, _movebox.Midz))); 
				/* I put them ahead of the player  */ 
			randomizeVelocity( 0.0f, 30.0f, false ); 

                        
			if ( pownergame != null ) //Then we know we added this to a game so pplayer() is valid 
				addForce( new cForceObjectSeek( Player, 0.5f ));

            int begf = Framework.randomOb.random(0, 171);
            int endf = Framework.randomOb.random(0, 171);

            if (begf > endf)
            {
                int temp = begf;
                begf = endf;
                endf = temp;
            }

			Sprite.setstate( State.Other, begf, endf, StateType.Repeat );


            _wrapflag = cCritter.BOUNCE;

		} 

		
		public override void update( ACView pactiveview, float dt ) 
		{ 
			base.update( pactiveview, dt ); //Always call this first
			if ( (_outcode & cRealBox3.BOX_HIZ) != 0 ) /* use bitwise AND to check if a flag is set. */ 
				delete_me(); //tell the game to remove yourself if you fall up to the hiz.
            if (Position.Z != Game.Border.Midz)
                moveToZ(Game.Border.Midz);
        } 

		// do a delete_me if you hit the left end 
	
		public override void die() 
		{ 
			Player.addScore( Value ); 
			base.die(); 
		} 

       public override bool IsKindOf( string str )
        {
            return str == "cCritter3Dcharacter" || base.IsKindOf( str );
        }
	
        public override string RuntimeClass
        {
            get
            {
                return "cCritter3Dcharacter";
            }
        }
	} 
	
	class cCritterTreasure : cCritter 
	{   // Try jumping through this hoop
		
		public cCritterTreasure( cGame pownergame ) : 
		base( pownergame ) 
		{ 
			/* The sprites look nice from afar, but bitmap speed is really slow
		when you get close to them, so don't use this. */ 
			cPolygon ppoly = new cPolygon( 24 ); 
			ppoly.Filled = false; 
			ppoly.LineWidthWeight = 0.5f;
			Sprite = ppoly; 
			_collidepriority = cCollider.CP_PLAYER + 1;
            /* Let this guy call collide on the
			   player, as his method is overloaded in a special way. */ 
			rotate( new cSpin( (float) Math.PI / 2.0f, new cVector3(0.0f, 0.0f, 1.0f) ));
            /* Trial and error shows this
			   rotation works to make it face the z diretion. */ 
			setRadius( cGame3D.TREASURERADIUS ); 
			FixedFlag = true; 
			moveTo( new cVector3( _movebox.Midx, _movebox.Midy, _movebox.Midz )); 
		} 

		
		public override bool collide( cCritter pcritter ) 
		{ 
			if ( contains( pcritter )) //disk of pcritter is wholly inside my disk 
			{
                Framework.snd.play(Sound.LoseLife); 
				pcritter.addScore( 100 ); 
				pcritter.addHealth( 1 ); 
				pcritter.moveTo( new cVector3( _movebox.Midx, _movebox.Loy, _movebox.Midz )); 
				return true; 
			} 
			else 
				return false; 
		} 

		//Checks if pcritter inside.
	
		public override int collidesWith( cCritter pothercritter ) 
		{ 
			if ( pothercritter.IsKindOf( "cCritter3DPlayer" )) 
				return cCollider.COLLIDEASCALLER; 
			else 
				return cCollider.DONTCOLLIDE; 
		} 

		/* Only collide
			with cCritter3DPlayer. */ 

       public override bool IsKindOf( string str )
        {
            return str == "cCritterTreasure" || base.IsKindOf( str );
        }
	
        public override string RuntimeClass
        {
            get
            {
                return "cCritterTreasure";
            }
        }
	} 

    //********************NEW CLASSES************************
    class cCritter2point5Dcharacter : cCritter3Dcharacter
    {
        float deathAge = 0;
        public virtual float DeathLingerTime { get { return .5f; } }
        public cCritter2point5Dcharacter(cGame pownergame)
            :base(pownergame)
        {

        }
        public override bool IsKindOf(string str)
        {
            return str == "cCritter2point5Dcharacter" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritter2point5Dcharacter";
            }
        }
        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            if ((deathAge != 0) && (Age > deathAge + DeathLingerTime))
                delete_me();
        }
        public override void die()
        {
            deathAge = Age;
        }
        public void turnRight()
        {
            Attitude = cMatrix3.xRotation((float)(3.0 * Math.PI / 2.0));
            lookAt(new cVector3(Game.Border.Hix, Position.Y, Position.Z));
        }
        public void turnLeft()
        {
            Attitude = cMatrix3.xRotation((float)(1.0 * Math.PI / 2.0));
            lookAt(new cVector3(Game.Border.Lox, Position.Y, Position.Z));
        }
    }

    class cCritter3DTurtle : cCritter2point5Dcharacter
    {
        bool onScreen = false;
        bool TurnedLeft = true;
        bool TurnedRight = false;
        const float SPEED = 3.0f;
        const float DEATH_LINGER = .5f;

        public override float DeathLingerTime { get { return DEATH_LINGER; } }
        public cCritter3DTurtle(cGame pownergame)
            : base(pownergame)
        {
            Sprite = new cSpriteQuake(ModelsMD2.Turtle);
            clearForcelist();
            addForce(new cForceGravity());
            addForce(new cForceDrag());
            turnLeft();
            /*
            addForce(new cForceGravity(turtleSpeed, leftPull));
            lookAt(new cVector3(Game.Border.Lox, Position.Y, Position.Z));
            facingLeft = true;
             * */
        }
        public override bool IsKindOf(string str)
        {
            return str == "cCritter3DTurtle" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DTurtle";
            }
        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            if (onScreen)
            {
                //lookAt(new cVector3(Player.Position.X, MoveBox.Loy, MoveBox.Midz));
                if (Player.Position.X < Position.X&&TurnedRight)
                {
                    //yaw((float)Math.PI);
                    turnLeft();
                    TurnedRight = false;
                    TurnedLeft = true;
                    //this.rotate(new cSpin((float)Math.PI,new cVector3(0,1,0)));(float)(3.0 * Math.PI / 2.0)
                }
                else if (Player.Position.X > Position.X&&TurnedLeft)
                {
                    //yaw((float)Math.PI);
                    turnRight();
                    TurnedRight = true;
                    TurnedLeft = false;
                }
                if (Position.Y > 1.0)
                    moveTo(new cVector3(Position.X, 1.0f, Position.Z));
            }
            else if (Math.Abs(Player.Position.X - Position.X) < 6.0f)
            {
                addForce(new cForceObjectSeek(Player, SPEED));
                onScreen = true;
            }
            //yaw(Velocity.angleBetween(AttitudeNormal));
            /*
            if (Velocity.IsPracticallyZero&&facingLeft)
            {
                clearForcelist();
                addForce(new cForceGravity());
                addForce(new cForceGravity(turtleSpeed, rightPull));
                lookAt(new cVector3(Game.Border.Hix, Position.Y, Position.Z));
                facingLeft = false;
            }
            else if (Velocity.IsPracticallyZero)
            {
                clearForcelist();
                addForce(new cForceGravity());
                addForce(new cForceGravity(turtleSpeed, leftPull));
                lookAt(new cVector3(Game.Border.Lox, Position.Y, Position.Z));
                facingLeft = true;
            }*/
        }

        public override bool collide(cCritter pother)
        {
            /*
            if (this.touch(pother) && (!pother.Equals(this)))
            {
                //MessageBox.Show("Collided with" + pother.RuntimeClass);
                if (facingLeft)
                {
                    moveTo(new cVector3(Position.X - 0.5f, Position.Y, Position.Z));
                    clearForcelist();
                    addForce(new cForceGravity());
                    addForce(new cForceGravity(1.0f,new cVector3(0.5f)));
                    lookAt(new cVector3(Game.Border.Hix, Position.Y, Position.Z));
                    facingLeft = false;
                }
                else if (!facingLeft)
                {
                    moveTo(new cVector3(Position.X + 0.5f, Position.Y, Position.Z));
                    clearForcelist();
                    addForce(new cForceGravity());
                    addForce(new cForceGravity(1.0f,new cVector3(-0.5f)));
                    lookAt(new cVector3(Game.Border.Lox, Position.Y, Position.Z));
                    facingLeft = true;
                }
            }*/
            return base.collide(pother);
        }

        public override void die()
        {
            base.die();
            clearForcelist();
            addForce(new cForceDrag());
            Sprite.ModelState = State.FallbackDie;
            Player.addScore(1);
        }
    }

    class cCritter3DPenguin : cCritter2point5Dcharacter
    {
        bool onScreen = false;
        bool TurnedLeft = true;
        bool TurnedRight = false;
        const float SPEED = 4.0f;
        const float DEATH_LINGER = .5f;

        public override float DeathLingerTime { get { return DEATH_LINGER; } }
        public cCritter3DPenguin(cGame pownergame)
            : base(pownergame)
        {
            Sprite = new cSpriteQuake(ModelsMD2.Penguin);
            clearForcelist();
            addForce(new cForceGravity());
            addForce(new cForceDrag());
            turnLeft();
        }
        public override bool IsKindOf(string str)
        {
            return str == "cCritter3DPenguin" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DPenguin";
            }
        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            if (onScreen)
            {
                if (Player.Position.X < Position.X&&TurnedRight)
                {
                    turnLeft();
                    TurnedRight = false;
                    TurnedLeft = true;
                }
                else if (Player.Position.X > Position.X&&TurnedLeft)
                {
                    turnRight();
                    TurnedRight = true;
                    TurnedLeft = false;
                }
                if (Position.Y > 1.0)
                    moveTo(new cVector3(Position.X, 1.0f, Position.Z));
            }
            else if (Math.Abs(Player.Position.X - Position.X) < 6.0f)
            {
                addForce(new cForceObjectSeek(Player, SPEED));
                onScreen = true;
            }
        }

        public override bool collide(cCritter pother)
        {
            return base.collide(pother);
        }

        public override void die()
        {
            base.die();
            clearForcelist();
            addForce(new cForceDrag());
            Sprite.ModelState = State.FallbackDie;
            Player.addScore(1);
        }
    }

    class cCritter3DFrog : cCritter2point5Dcharacter
    {
        bool onScreen = false;
        bool TurnedLeft = true;
        bool TurnedRight = false;
        const float SPEED = 3.0f;
        const float DEATH_LINGER = .5f;

        public override float DeathLingerTime { get { return DEATH_LINGER; } }
        public cCritter3DFrog(cGame pownergame)
            : base(pownergame)
        {
            Sprite = new cSpriteQuake(ModelsMD2.Frog);
            clearForcelist();
            addForce(new cForceGravity(5.0f));
            addForce(new cForceDrag());
            turnLeft();
            /*
            addForce(new cForceGravity(turtleSpeed, leftPull));
            lookAt(new cVector3(Game.Border.Lox, Position.Y, Position.Z));
            facingLeft = true;
             * */
        }
        public override bool IsKindOf(string str)
        {
            return str == "cCritter3DFrog" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DFrog";
            }
        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            if (onScreen)
            {
                if (Player.Position.X < Position.X&&TurnedRight)
                {
                    turnLeft();
                    TurnedRight = false;
                    TurnedLeft = true;
                }
                else if (Player.Position.X > Position.X&&TurnedLeft)
                {
                    turnRight();
                    TurnedRight = true;
                    TurnedLeft = false;
                }
                if (Position.Y > 1.0)
                    moveTo(new cVector3(Position.X, 1.0f, Position.Z));
            }
            else if (Math.Abs(Player.Position.X - Position.X) < 6.0f)
            {
                addForce(new cForceObjectSeek(Player, SPEED));
                onScreen = true;
            }
        }

        public override bool collide(cCritter pother)
        {
            return base.collide(pother);
        }

        public override void die()
        {
            base.die();
            clearForcelist();
            addForce(new cForceDrag());
            Sprite.ModelState = State.FallForwardDie;
            Player.addScore(1);
        }
    }

    class cCritter3DDungeonBoss : cCritter2point5Dcharacter
    {
        bool onScreen = false;
        bool TurnedLeft = true;
        bool TurnedRight = false;
        const float SPEED = 2.0f;
        const float DEATH_LINGER = .5f;

        public override float DeathLingerTime { get { return DEATH_LINGER; } }
        public cCritter3DDungeonBoss(cGame pownergame)
            : base(pownergame)
        {
            Sprite = new cSpriteQuake(ModelsMD2.Wario);
            clearForcelist();
            addForce(new cForceGravity());
            addForce(new cForceDrag());
            turnLeft();
        }
        public override bool IsKindOf(string str)
        {
            return str == "cCritter3DDungeonBoss" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DDungeonBoss";
            }
        }

        public override void update(ACView pactiveview, float dt)
        {
            base.update(pactiveview, dt);
            if (onScreen)
            {
                if (Player.Position.X < Position.X&&TurnedRight)
                {
                    turnLeft();
                    TurnedRight = false;
                    TurnedLeft = true;
                }
                else if (Player.Position.X > Position.X&&TurnedLeft)
                {
                    turnRight();
                    TurnedRight = true;
                    TurnedLeft = false;
                }
                if (Position.Y > 1.0)
                    moveTo(new cVector3(Position.X, 1.0f, Position.Z));
            }
            else if (Math.Abs(Player.Position.X - Position.X) < 5.0f)
            {
                addForce(new cForceObjectSeek(Player, SPEED));
                onScreen = true;
            }
        }

        public override bool collide(cCritter pother)
        {
            return base.collide(pother);
        }

        public override void die()
        {
            base.die();
            clearForcelist();
            addForce(new cForceDrag());
            Sprite.ModelState = State.FallbackDie;
            Player.addScore(1);
        }
    }

    class cCritter3DGoal : cCritter3Dcharacter
    {
        bool TurnedLeft = true;
        bool TurnedRight = false;
        public cCritter3DGoal(cGame pownergame)
            : base(pownergame)
        {
            Sprite = new cSpriteQuake(ModelsMD2.Luigi);
            Sprite.ModelState = State.Idle;
            clearForcelist();
            addForce(new cForceGravity());
            //turnLeft();
        }
        public override bool IsKindOf(string str)
        {
            return str == "cCritter3DGoal" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DGoal";
            }
        }

        public override void update(ACView pactiveview, float dt)
        {
            /*
            base.update(pactiveview, dt);
            if (Player.Position.X < Position.X && TurnedRight)
            {
                turnLeft();
                TurnedRight = false;
                TurnedLeft = true;
            }
            else if (Player.Position.X > Position.X && TurnedLeft)
            {
                turnRight();
                TurnedRight = true;
                TurnedLeft = false;
            }*/
        }

        public override bool collide(cCritter pother)
        {
            if (touch(pother))
            {
                MessageBox.Show("Thanks-a bro-a");
                return true;
            }
            else
                return false;
        }

        public override void die()
        {
            //Be immortal
        }
    }

    class cCritter3DKey : cCritterTreasure
    {
        public cCritter3DKey( cGame pownergame ) : 
		base( pownergame ) 
		{ 
            cSpriteIcon keySprite = new cSpriteIcon(BitmapRes.Key);
            Sprite = keySprite;
            moveTo(new cVector3(_movebox.Midx, _movebox.Midy, _movebox.Midz+0.5f)); 
		}
        public override bool IsKindOf(string str)
        {
            return str == "cCritter3DKey" || base.IsKindOf(str);
        }

        public override string RuntimeClass
        {
            get
            {
                return "cCritter3DKey";
            }
        }
        
        public override bool collide(cCritter pcritter)
        {
            if ( this.touch( pcritter ) && (pcritter==Player)) 
			{
                //MessageBox.Show("Make door collidable");
                
                this.die();
                ((cGame3D)Game).pwallGate.Open = true;
				return true;
			} 
			else 
				return false; 
        }
	} 
	
	//======================cGame3D========================== 
	
	class cGame3D : cGame 
	{ 
		public static readonly float TREASURERADIUS = 1.2f; 
		public static readonly float WALLTHICKNESS = 1.0f; 
		public static readonly float PLAYERRADIUS = 0.2f; 
		public static readonly float MAXPLAYERSPEED = 30.0f; 
		private cCritterTreasure _ptreasure; 
		private bool doorcollision;
        private bool wentThrough = false;
        private float startNewRoom;
        private int levelNumber = 1;
        public cCritterWallGate pwallGate;
		
		public cGame3D() 
		{
            MessageBox.Show("Press 'C' for 'Childs Mode'.");
            
            doorcollision = false; 
			_menuflags &= ~ cGame.MENU_BOUNCEWRAP; 
			_menuflags |= cGame.MENU_HOPPER; //Turn on hopper listener option.
			_spritetype = cGame.ST_MESHSKIN; 
			
            /* SET THE SIZE OF THE ROOM TO BE LONG AND NARROW ~JM */
            setBorder( 100.0f, 10.0f, 10.0f ); // size of the world
		
			cRealBox3 skeleton = new cRealBox3();
            skeleton.copy(_border);
			setSkyBox( skeleton );
		    /* In this world the coordinates are screwed up to match the screwed up
		       listener that I use.  I should fix the listener and the coords.
		       Meanwhile...
		       I am flying into the screen from HIZ towards LOZ, and
		       LOX below and HIX above and
		       LOY on the right and HIY on the left. */ 
            SkyBox.setSideTexture(cRealBox3.LOZ, BitmapRes.OverworldBackground, 8); // far wall 
            SkyBox.setSideSolidColor(cRealBox3.LOX, System.Drawing.ColorTranslator.FromHtml("#F8E0B0")); // left wall 
            SkyBox.setSideSolidColor(cRealBox3.HIX, System.Drawing.ColorTranslator.FromHtml("#F8E0B0")); // right wall 
            SkyBox.setSideSolidColor(cRealBox3.HIY, System.Drawing.ColorTranslator.FromHtml("#F8E0B0")); // ceiling 
            SkyBox.setSideSolidColor(cRealBox3.LOY, System.Drawing.Color.Black); // floor
		
			WrapFlag = cCritter.BOUNCE; 
			_seedcount = 0; 
			setPlayer( new cCritter3DPlayer( this )); 
            _ptreasure = new cCritter3DKey(this);
		
			/* In this world the x and y go left and up respectively, while z comes out of the screen.
		       A wall views its "thickness" as in the y direction, which is up here, and its
		       "height" as in the z direction, which is into the screen. */

			// First draw a wall with dy height resting on the bottom of the world.
			float zpos = 0.0f; /* Point on the z axis where we set down the wall.  0 would be center,
			                      halfway down the hall, but we can offset it if we like. */ 
			float height = 0.2f * _border.YSize; 
			float ycenter = -_border.YRadius + height / 2.0f; 
			float wallthickness = cGame3D.WALLTHICKNESS;
            
            /* THIS IS THE FLOOR FOR THE FIREBALLS TO BOUNCE OFF OF ~ JM */
            cCritterWall pwall = new cCritterWall
            ( 
				new cVector3( _border.Lox, _border.Loy + 0.1f, _border.Midz ), 
				new cVector3( _border.Midx + 3.0f, _border.Loy + 0.1f, _border.Midz), 
				height / 15.0f, // thickness param for wall's dy which goes perpendicular to the 
					            // baseline established by the frist two args, up the screen 
				wallthickness * 10.0f,  // height argument for this wall's dz  goes into the screen 
				this
            );
            cSpriteTextureBox pspritebox = new cSpriteTextureBox( pwall.Skeleton, BitmapRes.OverworldGround, 8 );
            pwall.Sprite = pspritebox; 
		
            /* THIS IS THE BRICKS AT THE BEGINNING OF THE LEVEL ~ JM */
            pwall = new cCritterWall
            (
                new cVector3(_border.Lox + 7.0f, _border.Midy - 1.0f, _border.Midz),
                new cVector3(_border.Lox + 17.0f, _border.Midy - 1.0f, _border.Midz),
                height / 2.0f, 
                wallthickness / 2.0f,
                this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.OverworldWall, 16);
            pwall.Sprite = pspritebox;

            /* THIS IS THE BRICKS "HILL" BEGINNING OF THE LEVEL ~ JM */
            // base tier
            pwall = new cCritterWall
            (
                new cVector3(_border.Lox + 21.0f, _border.Loy + 0.75f, _border.Midz),
                new cVector3(_border.Lox + 31.0f, _border.Loy + 0.75f, _border.Midz),
                height / 2.0f,
                wallthickness * 2.0f,
                this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.OverworldWall, 16);
            pwall.Sprite = pspritebox;
            // middle tier
            pwall = new cCritterWall
            (
                new cVector3(_border.Lox + 23.0f, _border.Loy + 1.75f, _border.Midz),
                new cVector3(_border.Lox + 29.0f, _border.Loy + 1.75f, _border.Midz),
                height / 2.0f,
                wallthickness,
                this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.OverworldWall, 8);
            pwall.Sprite = pspritebox;
            // top tier
            pwall = new cCritterWall
            (
                new cVector3(_border.Lox + 24.0f, _border.Loy + 2.75f, _border.Midz),
                new cVector3(_border.Lox + 27.0f, _border.Loy + 2.75f, _border.Midz),
                height / 2.0f,
                wallthickness / 2.0f,
                this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.OverworldWall, 4);
            pwall.Sprite = pspritebox;

            /* THIS IS THE "REGULAR" PIPE IN THE MIDDLE OF THE LEVEL*/
            pwall = new cCritterWall
            (
                new cVector3(_border.Midx - 8.0f, _border.Loy - 1.0f, zpos),
                new cVector3(_border.Midx - 8.0f, _border.Loy + 2.0f, zpos),
                height / 2.0f,
                wallthickness / 2.0f,
                this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.Pipe);
            pwall.Sprite = pspritebox;

			/* THIS IS THE ANGLED PIPE IN THE MIDDLE OF THE LEVEL*/
			pwall = new cCritterWall
            ( 
				new cVector3( _border.Midx, _border.Loy - 1.0f, zpos), 
				new cVector3( _border.Midx + 4.0f, _border.Loy + 2.0f, zpos), 
				height, 
				wallthickness / 2.0f, 
				this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.Pipe);
            pwall.Sprite = pspritebox;
		
            /* THIS HILL TOWARDS THE END OF THE LEVEL */
            // up the hill
            pwall = new cCritterWall
            (
                new cVector3(_border.Midx + 7.0f, _border.Loy, _border.Midz),
                new cVector3(_border.Midx + 30.0f, _border.Loy + 2.0f, _border.Midz),
                height / 20.0f,
                wallthickness * 10.0f,
                this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.OverworldGround, 2);
            pwall.Sprite = pspritebox;
            // down the hill
            pwall = new cCritterWall
            (
                new cVector3(_border.Midx + 30.0f, _border.Loy + 2.0f, _border.Midz),
                new cVector3(_border.Midx + 40.0f, _border.Loy, _border.Midz),
                height / 20.0f, 
                wallthickness * 10.0f, 
                this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.OverworldGround, 2);
            pwall.Sprite = pspritebox;

            /* THE END OF THE FLOOR */
            pwall = new cCritterWall
            (
                new cVector3(_border.Hix - 11.0f, _border.Loy + 0.1f, _border.Midz),
                new cVector3(_border.Hix, _border.Loy + 0.1f, _border.Midz),
                height / 15.0f,        // thickness param for wall's dy which goes perpendicular to the 
                                       // baseline established by the frist two args, up the screen 
                wallthickness * 10.0f, // height argument for this wall's dz  goes into the screen 
                this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.OverworldGround, 2);
            pwall.Sprite = pspritebox; 

            /* THIS IS THE DOOR AT THE END OF THE LEVEL */
			cCritterDoor pdwall = new cCritterDoor
            ( 
				new cVector3( _border.Hix - 0.25f, _border.Loy, _border.Midz ), 
				new cVector3( _border.Hix - 0.25f, _border.Loy + 2.0f, _border.Midz ), 
				0.1f, 2.0f, this
            ); 
			cSpriteTextureBox pspritedoor = new cSpriteTextureBox( pdwall.Skeleton, BitmapRes.Door, 4 ); 
			pdwall.Sprite = pspritedoor;

            /* CASTLE AT THE END ~ JM */
            // far wall
            pwall = new cCritterWall
            (
                new cVector3(_border.Hix - 0.5f, _border.Loy + 0.0f, _border.Midz - 2.0f),
                new cVector3(_border.Hix, _border.Loy + 0.0f, _border.Midz - 2.0f),
                height * 5.0f,
                wallthickness * 2.0f,
                this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.OverworldWall, 16);
            pwall.Sprite = pspritebox;
            // top wall
            pwall = new cCritterWall
            (
                new cVector3(_border.Hix - 0.125f, _border.Loy + 2.0f, _border.Midz),
                new cVector3(_border.Hix, _border.Loy + 2.0f, _border.Midz),
                height * 2.0f,
                wallthickness * 2.0f,
                this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.OverworldWall, 8);
            pwall.Sprite = pspritebox;
            // near wall
            pwall = new cCritterWall
            (
                new cVector3(_border.Hix - 0.5f, _border.Loy + 0.0f, _border.Midz + 2.0f),
                new cVector3(_border.Hix, _border.Loy + 0.0f, _border.Midz + 2.0f),
                height * 5.0f,
                wallthickness * 2.0f,
                this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.OverworldWall, 16);
            pwall.Sprite = pspritebox;


            //New gate class wall, need to find out what position to start it in and then where
            //  to move it
            
            pwallGate = new cCritterWallGate
            (
                new cVector3(_border.Midx + 5.0f, _border.Loy + 0.0f, _border.Midz - 20.0f),
                10.0f,
                new cVector3(_border.Midx + 5.0f, _border.Loy + 0.0f, _border.Midz),
                new cVector3(_border.Midx + 2.0f, _border.Loy + 0.0f, _border.Midz),
                10.0f,
                2.0f,
                this
            );
            pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CastleWall, 16);
            pwallGate.Sprite = pspritebox;

            Player.moveTo(new cVector3(_border.Lox + 2.0f, 0.0f, 0.0f));
            Player.rotateAttitude((float)Math.PI / -2.0f);

            // insert turtles
            cCritter3DTurtle t1 = new cCritter3DTurtle(this);
            cCritter3DTurtle t2 = new cCritter3DTurtle(this);
            cCritter3DTurtle t3 = new cCritter3DTurtle(this);
            cCritter3DTurtle t4 = new cCritter3DTurtle(this);
            cCritter3DTurtle t5 = new cCritter3DTurtle(this);
            Biota.Add(t1);
            Biota.Add(t2);
            Biota.Add(t3);
            Biota.Add(t4);
            Biota.Add(t5);
            t1.moveTo(new cVector3(_border.Lox + 4.0f, _border.Loy, _border.Midz));
            t2.moveTo(new cVector3(_border.Lox + 18.0f, _border.Hiy, _border.Midz));
            t3.moveTo(new cVector3(_border.Midx -2.0f, _border.Hiy, _border.Midz));
            t4.moveTo(new cVector3(_border.Midx + 30.0f, _border.Hiy, _border.Midz));
            t5.moveTo(new cVector3(_border.Hix - 5.0f, _border.Hiy, _border.Midz));
		}

        public void setRoomToLevel( )
        {
            
            Biota.purgeCritters("cCritterWall");
            Biota.purgeCritters("cCritter3Dcharacter");
            Biota.purgeCritters("cCritterBullet");
            setBorder(100.0f, 10.0f, 10.0f); 
	        cRealBox3 skeleton = new cRealBox3();
            skeleton.copy( _border );
	        setSkyBox(skeleton);
            float height = 0.1f * _border.YSize;
            float wallthickness = cGame3D.WALLTHICKNESS;



            cCritterWall pwall;
            cSpriteTextureBox pspritebox;

           // _seedcount = 0;
            Player.moveTo(new cVector3(Border.Lox, 0.0f, 0.0f));
            Player.setMoveBox(new cRealBox3(100.0f, 10.0f, 10.0f));
            float zpos = 0.0f; /* Point on the z axis where we set down the wall.  0 would be center,
			                      halfway down the hall, but we can offset it if we like. */
            float ycenter = -_border.YRadius + height / 2.0f;

            /* THIS IS THE DOOR THAT IS ALWAYS AT THE END */
            cCritterDoor pdwall = new cCritterDoor
            (
                new cVector3(_border.Hix, _border.Loy, _border.Midz),
                new cVector3(_border.Hix, _border.Midy - 3, _border.Midz),
                0.4f, 2, this
            );
            cSpriteTextureBox pspritedoor = new cSpriteTextureBox(pdwall.Skeleton, BitmapRes.Door, 4);
            pdwall.Sprite = pspritedoor;

            /* THIS IS THE FLOOR FOR THE FIREBALLS TO BOUNCE OFF OF ~ JM */
            pwall = new cCritterWall
            (
                new cVector3(_border.Lox, _border.Loy + 0.1f, _border.Midz),
                new cVector3(_border.Hix, _border.Loy + 0.1f, _border.Midz),
                height / 15, // thickness param for wall's dy which goes perpendicular to the 
                             // baseline established by the frist two args, up the screen 
                wallthickness * 10.0f,  // height argument for this wall's dz  goes into the screen 
                this
            );

            // ////////////////////////////////////////////////////////////////////////////////////
            // we've left level 1 and are going to level 2 (underwater)
            // ////////////////////////////////////////////////////////////////////////////////////
            if (levelNumber == 1)
            {
                // clear out the baddies
                Biota.purgeNonPlayerCritters();

                _ptreasure = new cCritter3DKey(this);

                // add in the frogs
                // ADD IN ALL OF THE BAD GUYS
                cCritter3DFrog f1 = new cCritter3DFrog(this);
                cCritter3DFrog f2 = new cCritter3DFrog(this);
                cCritter3DFrog f3 = new cCritter3DFrog(this);
                cCritter3DFrog f4 = new cCritter3DFrog(this);
                cCritter3DFrog f5 = new cCritter3DFrog(this);
                Biota.Add(f1);
                Biota.Add(f2);
                Biota.Add(f3);
                Biota.Add(f4);
                Biota.Add(f5);
                f1.moveTo(new cVector3(_border.Lox + 10.0f, _border.Hiy, _border.Midz));
                f2.moveTo(new cVector3(_border.Lox + 20.0f, _border.Hiy, _border.Midz));
                f3.moveTo(new cVector3(_border.Midx, _border.Hiy, _border.Midz));
                f4.moveTo(new cVector3(_border.Midx + 25.0f, _border.Hiy, _border.Midz));
                f5.moveTo(new cVector3(_border.Hix - 5.0f, _border.Hiy, _border.Midz));
                
                SkyBox.setSideSolidColor(cRealBox3.HIZ, Color.Transparent); //Make the near HIZ transparent
                SkyBox.setSideTexture(cRealBox3.LOZ, BitmapRes.UnderwaterBackground, 8); //Far wall
                SkyBox.setSideSolidColor(cRealBox3.LOX, System.Drawing.ColorTranslator.FromHtml("#0060B8")); //left wall 
                SkyBox.setSideSolidColor(cRealBox3.HIX, System.Drawing.ColorTranslator.FromHtml("#0060B8")); //right wall 
                SkyBox.setSideSolidColor(cRealBox3.HIY, System.Drawing.ColorTranslator.FromHtml("#0060B8")); //ceiling

                /* THIS IS THE FLOOR FOR THE FIREBALLS TO BOUNCE OFF OF ~ JM */
                pwall = new cCritterWall
                (
                    new cVector3(_border.Lox, _border.Loy + 0.1f, _border.Midz),
                    new cVector3(_border.Hix, _border.Loy + 0.1f, _border.Midz),
                    height / 15.0f, // thickness param for wall's dy which goes perpendicular to the 
                    // baseline established by the frist two args, up the screen 
                    wallthickness * 10.0f,  // height argument for this wall's dz  goes into the screen 
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterGround, 8);
                pwall.Sprite = pspritebox;

                /* UNDERWATER HILLS */
                // go uphill
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx - 45.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Midx - 30.0f, _border.Loy + 3.0f, _border.Midz),
                    height / 20.0f,
                    wallthickness * 10.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterGround, 2);
                pwall.Sprite = pspritebox;
                // go downhill
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx - 30.0f, _border.Loy + 3.0f, _border.Midz),
                    new cVector3(_border.Midx - 10.0f, _border.Loy + 0.5f, _border.Midz),
                    height / 20.0f,
                    wallthickness * 10.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterGround, 2);
                pwall.Sprite = pspritebox;
                // go uphill (now past the middle)
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx - 15.0f, _border.Loy + 0.5f, _border.Midz),
                    new cVector3(_border.Midx + 10.0f, _border.Loy + 2.0f, _border.Midz),
                    height / 20.0f,
                    wallthickness * 10.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterGround, 2);
                pwall.Sprite = pspritebox;
                // go back down the hill (at close to the end)
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx + 10.0f, _border.Loy + 2.0f, _border.Midz),
                    new cVector3(_border.Midx + 35.0f, _border.Loy, _border.Midz),
                    height / 20.0f,
                    wallthickness * 10.0f,
                    this
                );

                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterGround, 2);
                pwall.Sprite = pspritebox;

                /* THIS IS THE BRICKS AT THE BEGINNING OF THE LEVEL ON THE FLOOR */
                pwall = new cCritterWall
                (
                    new cVector3(_border.Lox + 3.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Lox + 7.0f, _border.Loy, _border.Midz),
                    height + 2.0f,
                    wallthickness / 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterWall, 16);
                pwall.Sprite = pspritebox;

                /* LONGISH WALL IN THE BEGINNING */
                pwall = new cCritterWall
                (
                    new cVector3(_border.Lox + 15.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Lox + 33.0f, _border.Loy, _border.Midz),
                    height + 6.0f,
                    wallthickness / 4.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterWall, 16);
                pwall.Sprite = pspritebox;

                /* THIS IS THE ANGLED PIPE IN THE MIDDLE OF THE LEVEL COMING FROM THE CEILING */
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx, _border.Hiy - 5.0f, zpos),
                    new cVector3(_border.Midx + 3.0f, _border.Hiy + 1.0f, zpos),
                    height,
                    wallthickness / 2.0f,
                    this
                );
                cSpriteTextureBox stb = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.Pipe);
                pwall.Sprite = stb;

                /* TALL COLUMN AFTER PIPE */
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx + 9.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Midx + 10.0f, _border.Loy, _border.Midz),
                    height + 8.0f,
                    wallthickness,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterWall, 4);
                pwall.Sprite = pspritebox;

                /* MIDSIZE COLUMN AFTER PIPE */
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx + 15.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Midx + 16.0f, _border.Loy, _border.Midz),
                    height + 5.0f,
                    wallthickness * 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterWall, 4);
                pwall.Sprite = pspritebox;

                /* TALLER COLUMN AFTER PIPE */
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx + 24.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Midx + 26.0f, _border.Loy, _border.Midz),
                    height + 10.0f,
                    wallthickness / 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterWall, 4);
                pwall.Sprite = pspritebox;

                /* CASTLE AT THE END ~ JM */
                // far wall
                pwall = new cCritterWall
                (
                    new cVector3(_border.Hix - 0.5f, _border.Loy + 0.0f, _border.Midz - 2.0f),
                    new cVector3(_border.Hix, _border.Loy + 0.0f, _border.Midz - 2.0f),
                    height * 10.0f,
                    wallthickness * 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterWall, 16);
                pwall.Sprite = pspritebox;
                // top wall
                pwall = new cCritterWall
                (
                    new cVector3(_border.Hix - 0.125f, _border.Loy + 2.0f, _border.Midz),
                    new cVector3(_border.Hix, _border.Loy + 2.0f, _border.Midz),
                    height * 8.0f,
                    wallthickness * 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterWall, 8);
                pwall.Sprite = pspritebox;
                // near wall
                pwall = new cCritterWall
                (
                    new cVector3(_border.Hix - 0.5f, _border.Loy + 0.0f, _border.Midz + 2.0f),
                    new cVector3(_border.Hix, _border.Loy + 0.0f, _border.Midz + 2.0f),
                    height * 10.0f,
                    wallthickness * 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.UnderwaterWall, 16);
                pwall.Sprite = pspritebox;

                // clear gravity and reduce gravity because we're underwater
                Player.clearForcelist();
                Player.addForce(new cForceGravity(5.0f));

            }
            // ////////////////////////////////////////////////////////////////////////////////////
            // we've left level 2 and are going to level 3 (the cave)
            // ////////////////////////////////////////////////////////////////////////////////////

            else if (levelNumber == 2)
            {
                // clear out the previous baddies
                Biota.purgeNonPlayerCritters();

                _ptreasure = new cCritter3DKey(this);

                SkyBox.setSideSolidColor(cRealBox3.HIZ, Color.Transparent); //Make the near HIZ transparent
                SkyBox.setSideTexture(cRealBox3.LOZ, BitmapRes.CaveBackground, 8); //Far wall
                SkyBox.setSideSolidColor(cRealBox3.LOX, System.Drawing.ColorTranslator.FromHtml("#908878")); //left wall 
                SkyBox.setSideSolidColor(cRealBox3.HIX, System.Drawing.ColorTranslator.FromHtml("#908878")); //right wall
                SkyBox.setSideTexture(cRealBox3.HIY, BitmapRes.CaveFloor, 8); //ceiling

                // ADD IN ALL OF THE BAD GUYS
                cCritter3DPenguin p1 = new cCritter3DPenguin(this);
                cCritter3DPenguin p2 = new cCritter3DPenguin(this);
                cCritter3DPenguin p3 = new cCritter3DPenguin(this);
                cCritter3DPenguin p4 = new cCritter3DPenguin(this);
                cCritter3DPenguin p5 = new cCritter3DPenguin(this);
                Biota.Add(p1);
                Biota.Add(p2);
                Biota.Add(p3);
                Biota.Add(p4);
                Biota.Add(p5);
                p1.moveTo(new cVector3(_border.Lox + 10.0f, _border.Hiy, _border.Midz));
                p2.moveTo(new cVector3(_border.Lox + 20.0f, _border.Hiy, _border.Midz));
                p3.moveTo(new cVector3(_border.Midx, _border.Hiy, _border.Midz));
                p4.moveTo(new cVector3(_border.Midx + 25.0f, _border.Hiy, _border.Midz));
                p5.moveTo(new cVector3(_border.Hix - 5.0f, _border.Hiy, _border.Midz));

                /* THIS IS THE FLOOR FOR THE FIREBALLS TO BOUNCE OFF OF ~ JM */
                pwall = new cCritterWall
                (
                    new cVector3(_border.Lox, _border.Loy + 0.1f, _border.Midz),
                    new cVector3(_border.Hix, _border.Loy + 0.1f, _border.Midz),
                    height / 15.0f, // thickness param for wall's dy which goes perpendicular to the 
                                 // baseline established by the frist two args, up the screen 
                    wallthickness * 10.0f,  // height argument for this wall's dz  goes into the screen 
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CaveFloor, 16);
                pwall.Sprite = pspritebox;

                /* CAVE HILLS AND CEILINGS */
                // go uphill
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx - 45.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Midx - 25.0f, _border.Loy + 3.5f, _border.Midz),
                    height / 10.0f,
                    wallthickness * 10.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CaveFloor, 2);
                pwall.Sprite = pspritebox;
                // go downhill
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx - 25.0f, _border.Loy + 3.5f, _border.Midz),
                    new cVector3(_border.Midx - 10.0f, _border.Loy - 0.5f, _border.Midz),
                    height / 20.0f,
                    wallthickness * 10.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CaveFloor, 2);
                pwall.Sprite = pspritebox;

                // "stalactite" down
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx + 5.0f, _border.Hiy, _border.Midz),
                    new cVector3(_border.Midx + 10.0f, _border.Midy - 0.5f, _border.Midz),
                    height / 10.0f,
                    wallthickness * 10.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CaveFloor, 2);
                pwall.Sprite = pspritebox;
                // "stalactite" up
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx + 10.0f, _border.Midy - 0.5f, _border.Midz),
                    new cVector3(_border.Midx + 15.0f, _border.Hiy, _border.Midz),
                    height / 10.0f,
                    wallthickness * 10.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CaveFloor, 2);
                pwall.Sprite = pspritebox;

                /* CASTLE AT THE END ~ JM */
                // far wall
                pwall = new cCritterWall
                (
                    new cVector3(_border.Hix - 0.5f, _border.Loy + 0.0f, _border.Midz - 2.0f),
                    new cVector3(_border.Hix, _border.Loy + 0.0f, _border.Midz - 2.0f),
                    height * 5.0f,
                    wallthickness * 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CaveWall, 16);
                pwall.Sprite = pspritebox;
                // top wall
                pwall = new cCritterWall
                (
                    new cVector3(_border.Hix - 0.125f, _border.Loy + 2.0f, _border.Midz),
                    new cVector3(_border.Hix, _border.Loy + 2.0f, _border.Midz),
                    height * 2.0f,
                    wallthickness * 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CaveWall, 8);
                pwall.Sprite = pspritebox;
                // near wall
                pwall = new cCritterWall
                (
                    new cVector3(_border.Hix - 0.5f, _border.Loy + 0.0f, _border.Midz + 2.0f),
                    new cVector3(_border.Hix, _border.Loy + 0.0f, _border.Midz + 2.0f),
                    height * 5.0f,
                    wallthickness * 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CaveWall, 16);
                pwall.Sprite = pspritebox;

                // clear gravity and reduce gravity because we're back on land
                Player.clearForcelist();
                Player.addForce(new cForceGravity(50.0f));
            }

            // ////////////////////////////////////////////////////////////////////////////////////
            // we've left level 3 and are going to level 4 (castle)
            // ////////////////////////////////////////////////////////////////////////////////////

            else if (levelNumber == 3)
            {
                // clear out an bad guys
                Biota.purgeNonPlayerCritters();

                // get rid of the door
               // pdwall.die();
                
                SkyBox.setSideSolidColor(cRealBox3.HIZ, Color.Transparent); //Make the near HIZ transparent
                SkyBox.setSideTexture(cRealBox3.LOZ, BitmapRes.CastleBackground, 8); //Far wall
                SkyBox.setSideSolidColor(cRealBox3.LOX, System.Drawing.ColorTranslator.FromHtml("#908878")); //left wall 
                SkyBox.setSideSolidColor(cRealBox3.HIX, System.Drawing.ColorTranslator.FromHtml("#908878")); //right wall 
                SkyBox.setSideSolidColor(cRealBox3.HIY, System.Drawing.ColorTranslator.FromHtml("#908878")); //ceiling
                SkyBox.setSideSolidColor(cRealBox3.LOY, System.Drawing.Color.Red); // floor

                // add in the enemies
                cCritter3DTurtle t6 = new cCritter3DTurtle(this);
                cCritter3DTurtle t7 = new cCritter3DTurtle(this);
                cCritter3DTurtle t8 = new cCritter3DTurtle(this);
                cCritter3DTurtle t9 = new cCritter3DTurtle(this);
                cCritter3DDungeonBoss wario = new cCritter3DDungeonBoss(this);
                Biota.Add(t6);
                Biota.Add(t7);
                Biota.Add(t8);
                Biota.Add(t9);
                Biota.Add(wario);
                t6.moveTo(new cVector3(_border.Lox + 6.0f, _border.Midy, _border.Midz));
                t7.moveTo(new cVector3(_border.Lox + 33.0f, _border.Hiy, _border.Midz));
                t8.moveTo(new cVector3(_border.Midx - 28.0f, _border.Hiy, _border.Midz));
                t9.moveTo(new cVector3(_border.Midx + 10.0f, _border.Hiy, _border.Midz));
                wario.moveTo(new cVector3(_border.Hix - 21.0f, _border.Midy+7.0f, _border.Midz));

                // luigi's saved
                cCritter luigi = new cCritter3Dcharacter(this);
                luigi.Sprite = new cSpriteQuake(ModelsMD2.Luigi);
                luigi.Sprite.ModelState = State.Idle;
                luigi.moveTo(new cVector3(_border.Hix - 10.0f, _border.Midy, _border.Midz));

                /* THIS IS THE FLOOR FOR THE FIREBALLS TO BOUNCE OFF OF ~ JM */
                pwall = new cCritterWall
                (
                    new cVector3(_border.Lox, _border.Loy + 0.1f, _border.Midz),
                    new cVector3(_border.Lox + 8.0f, _border.Loy + 0.1f, _border.Midz),
                    height / 15.0f,
                    wallthickness * 10.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CastleFloor, 8);
                pwall.Sprite = pspritebox;

                /* FLOORS */
                // starting block
                pwall = new cCritterWall
                (
                    new cVector3(_border.Lox + 4.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Lox + 8.0f, _border.Loy, _border.Midz),
                    height * 2.0f,
                    wallthickness * 5.0f, 
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CastleFloor, 8);
                pwall.Sprite = pspritebox;

                // mind the gap

                // second slightly taller wall (16% of the way to end)
                pwall = new cCritterWall
                (
                    new cVector3(_border.Lox + 11.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Lox + 16.0f, _border.Loy, _border.Midz),
                    height * 3.5f,
                    wallthickness * 7.5f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CastleFloor, 8);
                pwall.Sprite = pspritebox;

                // mind the gap

                // floating wall
                pwall = new cCritterWall
                (
                    new cVector3(_border.Lox + 18.0f, _border.Midy - 2.2f, _border.Midz),
                    new cVector3(_border.Lox + 28.0f, _border.Midy - 2.2f, _border.Midz),
                    height * 2.0f,
                    wallthickness,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.OverworldWall, 8);
                pwall.Sprite = pspritebox;

                // little floor
                pwall = new cCritterWall
                (
                    new cVector3(_border.Lox + 31.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Lox + 36.0f, _border.Loy, _border.Midz),
                    height * 2.0f,
                    wallthickness * 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CastleFloor, 4);
                pwall.Sprite = pspritebox;

                // big slider -- up
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx - 14.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Midx + 1.0f, _border.Midy, _border.Midz),
                    height * 4.0f,
                    wallthickness * 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CastleWall, 16);
                pwall.Sprite = pspritebox;
                // big slider -- down
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx - 1.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Midx + 17.0f, _border.Midy, _border.Midz),
                    height * 4.0f,
                    wallthickness * 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CastleWall, 16);
                pwall.Sprite = pspritebox;

                // beginning of bridge
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx + 19.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Midx + 21.0f, _border.Loy, _border.Midz),
                    height * 4.0f,
                    wallthickness * 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CastleWall, 8);
                pwall.Sprite = pspritebox;
                // bridge
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx + 20.0f, _border.Loy + 2.0f, _border.Midz),
                    new cVector3(_border.Midx + 32.0f, _border.Loy + 2.0f, _border.Midz),
                    height / 2.0f,
                    wallthickness,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.OverworldWall, 8);
                pwall.Sprite = pspritebox;
                // end of bridge
                pwall = new cCritterWall
                (
                    new cVector3(_border.Midx + 31.0f, _border.Loy, _border.Midz),
                    new cVector3(_border.Midx + 33.0f, _border.Loy, _border.Midz),
                    height * 4.0f,
                    wallthickness * 2.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CastleWall, 8);
                pwall.Sprite = pspritebox;

                // the end!
                pwall = new cCritterWall
                (
                    new cVector3(_border.Hix - 17.0f, _border.Loy + 0.1f, _border.Midz),
                    new cVector3(_border.Hix, _border.Loy + 0.1f, _border.Midz),
                    height * 2.0f,
                    wallthickness * 10.0f,
                    this
                );
                pspritebox = new cSpriteTextureBox(pwall.Skeleton, BitmapRes.CastleFloor, 8);
                pwall.Sprite = pspritebox;
            }

            wentThrough = true;
            levelNumber++;
            startNewRoom = Age;
        }
		
		public override void seedCritters() 
		{
			//Biota.purgeCritters( "cCritterBullet" ); 
			//Biota.purgeCritters( "cCritter3Dcharacter" );
            //for (int i = 0; i < _seedcount; i++) 
				//new cCritter3Dcharacter( this );
            Player.moveTo(new cVector3(Border.Lox + 1.0f, Border.Loy, Border.Midz));
            Player.rotate(new cSpin(90.0f));
				/* We start at hiz and move towards	loz */ 
		} 

		
		public void setdoorcollision( ) { doorcollision = true; } 
		
		public override ACView View 
		{
            set
            {
                base.View = value; //You MUST call the base class method here.
                value.setUseBackground(ACView.FULL_BACKGROUND);
                /* The background type can be
			       ACView.NO_BACKGROUND, ACView.SIMPLIFIED_BACKGROUND, or 
			       ACView.FULL_BACKGROUND, which often means: nothing, lines, or
			       planes&bitmaps, depending on how the skybox is defined. */
                value.pviewpointcritter().Listener = new cListenerViewerRide();
            }
		} 

		
		public override cCritterViewer Viewpoint 
		{ 
            set
            {
			    if ( value.Listener.RuntimeClass == "cListenerViewerRide" ) 
			    { 
                    value.setViewpoint(new cVector3(0.0f, 0.0f, 5.0f), new cVector3(0.0f, 0.0f, 5.0f)); 
					//Always make some setViewpoint call simply to put in a default zoom.
				    value.zoom( 0.0f ); //Wideangle
				    cListenerViewerRide prider = ( cListenerViewerRide )( value.Listener); 
				    prider.Offset = (new cVector3( 0.0f, -5.0f, 0.0f)); /* This offset is in the coordinate
				    system of the player, where the negative X axis is the negative of the
				    player's tangent direction, which means stand right behind the player. */ 
			    } 
			    else //Not riding the player.
			    { 
				    value.zoom( 1.0f ); 
				    /* The two args to setViewpoint are (directiontoviewer, lookatpoint).
				    Note that directiontoviewer points FROM the origin TOWARDS the viewer. */ 
				    value.setViewpoint( new cVector3( 0.0f, 0.3f, 1.0f ), _border.Center); 
			    }
            }
		} 

		/* Move over to be above the
			lower left corner where the player is.  In 3D, use a low viewpoint low looking up. */ 
	
		public override void adjustGameParameters() 
		{
		// (1) End the game if the player is dead 
			if ( (Health == 0) && !_gameover ) //Player's been killed and game's not over.
			{ 
				_gameover = true; 
				Player.addScore( _scorecorrection ); // So user can reach _maxscore  
                Framework.snd.play(Sound.LoseLife);
                return ; 
			} 
		// (2) Also don't let the the model count diminish.
		//     (need to recheck propcount in case we just called seedCritters).
			int modelcount = Biota.count( "cCritter3Dcharacter" ); 
			int modelstoadd = _seedcount - modelcount; 
			for ( int i = 0; i < modelstoadd; i++) 
				new cCritter3Dcharacter( this ); 
		// (3) Maybe check some other conditions.

            if (wentThrough && (Age - startNewRoom) > 2.0f)
            {
                //MessageBox.Show("What an idiot.");
                wentThrough = false;
            }

            if (doorcollision == true)
            {
                if (levelNumber == 4)
                {
                    _gameover = true;
                    MessageBox.Show("Thank-a you so much-a for-a playing our game-a!");
                    levelNumber++;
                    return;
                }
                else
                {
                    setRoomToLevel();
                    Framework.snd.play(Sound.OpenDoor);
                    doorcollision = false;
                }
            }
		} 
		
		public override bool upIsZ(){ return false; } 
		
	} 
	
}