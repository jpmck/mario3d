// For AC Framework 1.2, I've added some default parameters -- JC

using System;
using System.Drawing;
using OpenTK.Input;
using System.Windows.Forms;

namespace ACFramework
{ 
	
	class cCritterBullet : cCritter 
	{ 
		public static readonly new float FIXEDLIFETIME = 3.0f; //Life in seconds before spontaneous decay, currently 3.0.
		public static readonly new float MAXSPEED = 22.0f; //Greater than cCritter::MAXSPEED. Currently 22.0.
		public static readonly bool DIEATEDGES = true; //Default _dieatedges is TRUE for disappear at edges.
		public static readonly float BULLETSPEED = 10.0f; 
	//Serialized fields 
		protected int _hitstrength; //How much damage it does.
		protected int _shooterindex; //Helper used only in Serialize 
		protected bool _dieatedges; /*  If TRUE this keeps bullets from bouncing or wrapping,	note that it
	also makes the critters unable to fire when they are really near an edge. */ 
	cCritterArmed _pshooter;
	//Nonserialized pointer ref.
		
		public cCritterBullet() 
        {
		    _pshooter = null; 
		    _shooterindex = cBiota.NOINDEX; 
		    _hitstrength = 1; 
		    _dieatedges = true; 
			_defaultprismdz = cSprite.BULLETPRISMDZ; 
			_value = 0; 
			_usefixedlifetime = true;
            _fixedlifetime = FIXEDLIFETIME; 
            _collidepriority = cCollider.CP_BULLET; /* Don't use the setCollidePriority mutator, as that
			forces a call to pgame()->buildCollider(); */ 
			_maxspeed = cCritterBullet.MAXSPEED; 
			Speed = cCritterBullet.BULLETSPEED;
            cSpriteSphere bulletsprite = new cSpriteSphere(cCritter.BULLETRADIUS, 6, 6);
            bulletsprite.FillColor = Color.Yellow; 
			Sprite = bulletsprite; /* Also sets cSprite._prismdz to cCritter._defaultprismdz, which we
			set to CritterWall.BULLETPRISMDZ above. */
        }

        public virtual cCritterBullet Create()
        {
            return new cCritterBullet();
        }

		
		public override void destruct() 
		{
            base.destruct();
			if ( _pshooter != null ) /* The cCritteArmed destructor sets _pshooter to NULL in its destrucor.  So if
							_pshooter isn't NULL, then it's still a good pointer. */ 
				_pshooter.removeBullet( this ); 
		} 
		
		public virtual void initialize( cCritterArmed pshooter ) 
		{ 
			_pshooter = pshooter; 
			setMoveBox( _pshooter.OwnerBiota.border()); /* Be sure to call setMoveBox before setVelocity,
			as setVelocity generates a call to fixNormalAndBinormal which looks at _movebox to see
			if it's ok if you happen to have a 3D velocity. */ 
			DragBox = _pshooter.OwnerBiota.border(); 
			Attitude = _pshooter.Attitude; //Orient the bullet like the shooter, fix position below.
            Tangent = _pshooter.AimVector; /* This call uses the _bulletspeed set by the
				cBullet constructor and the direction of the aim.  We choose NOT to add 
				_pshooter->velocity() to the new velocity. */ 
			setTarget( _pshooter.Target); 
			WrapFlag = _pshooter.WrapFlag; 
			cVector3 start = _pshooter.Position; //position() 
				/* I want to start the bullet out at the tip of the gun, with the provision
				that in any case I will I start it out far enough so that it's not touching
				the shooter. */ 
			float bulletdistance1 = _pshooter.GunLength * _pshooter.Radius; 
			float bulletdistance2 = pshooter.Radius + 1.5f * BULLETRADIUS; /* Need the 1.5 for enough
			room. Otherwise when the simulation is slow you may still touch. */ 
			float bulletdistance = ( bulletdistance1 > bulletdistance2 )? bulletdistance1
                : bulletdistance2; 
			cVector3 end = start.add( _pshooter.AimVector.mult( bulletdistance ) );
            end = end.add(new cVector3(0.0f, 1.0f, 0.0f));
			moveTo( end ); //Do this instead of just setting _position so as to fix wrapposition 
        } 

	
	//Accessor 
	
	// Mutator (don't call this unless you need to call it from ~cCritterArmed -- JC ) 
		
		public void nullTheShooter( ) { _pshooter = null; } 
	//Overloaded methods 
		
		public new void replicate(){} //Don't replicate bullets because then there's confusion about pshooter.
		
		public override void update( ACView pactiveview, float dt ) 
		{
            base.update( pactiveview, dt ); /* Feels force, also checks _age against _lifetime. */
            if (_baseAccessControl == 1)
                return;
            if ( (_outcode != 0) && _dieatedges) /* _outcode nonzero means near an edge.  This keeps bullets
			from bouncing or wrapping,	but it also makes the critters unable tofire when they are 
			really near an edge. */ 
			{ 
				delete_me(); 
				return ; 
			} 
		} 

		
		public override int collidesWith( cCritter pcritter ) 
		{ 
		/* Don't bump into other bullets from your shooter. If your shooter is dead (NULL),
	you'll also ignore bullets from other dead shooters.  Note that the cast in the 
	second clause of the && will crash C++ if the condition in the first clause isn't
	true.  But C++ doesn't evaluate the second clause of a && if the first clause is
	FALSE.  */ 
			if ( pcritter.IsKindOf( "cCritterBullet" ) && // C++ bails if FALSE 
				(( cCritterBullet ) pcritter ).Shooter == _pshooter ) // Can be NULL 
					return cCollider.DONTCOLLIDE; 
		//Don't bump into your shooter.
			if ( pcritter == _pshooter ) 
				return cCollider.DONTCOLLIDE; 
			return base.collidesWith( pcritter ); 
		} 

		
		public override bool collide( cCritter pcritter ) 
		{ 
		//If you hit a target, damage it and die.
            if (_baseAccessControl == 1)
                return base.collide(pcritter);
            if ( isTarget( pcritter )) 
			{ 
				if ( !touch( pcritter )) 
					return false; 
				int hitscore = pcritter.damage( _hitstrength ); 
				delete_me(); //Makes a service request, but you won't go away yet.
				if ( _pshooter != null ) //Possible that _pshooter has died, is NULL.
					_pshooter.addScore( hitscore ); 
				return true; 
			} 
		//Bounce off or everything else.
			return base.collide( pcritter ); //Bounce off non-target critters 
		} 

		
	//Special methods 
	
		public virtual bool isTarget( cCritter pcritter ) 
		{ 
		//Collide already rules out the _pshooter and bullets from the same shooter.
		//For now we'll view everything else except a wall as a target.
			if ( pcritter.RuntimeClass == "cCritterWall" ) /* Don't blow up any
				basic walls.  For now we WILL blow up children of walls, thinking ahead towards
				shooting out of cages, but if you don't want to do that, change this line to
				use IsKindOf. */ 
				return false; 
			return true; 
		} 

		/* Tells me if this is something I want to
			blow up.  Default is to blow up everything except (a) the shooter, (b) bullets of the
			same class as the shooter's bullets, or (c) walls. */ 
	
        public override bool IsKindOf( string str )
        {
            return str == "cCritterBullet" || base.IsKindOf( str );
        }

		public virtual cCritterArmed Shooter
		{
			get
				{ return _pshooter; }
		}

        public override string RuntimeClass
		{
			get
            {
                return "cCritterBullet";
            }
		}


    } 
	
	class cCritterBulletRubber : cCritterBullet 
	{ 
		public static readonly new bool DIEATEDGES = false; //If TRUE they disappear at edges, no bounce or wrap.
		public static readonly new float DENSITY = 100.0f; 
		
		public cCritterBulletRubber() 
		{ 
			_dieatedges = cCritterBulletRubber.DIEATEDGES;
            Density = cCritterBulletRubber.DENSITY; /* Use setDensity so you fixMass. */ 
			_collidepriority = cCollider.CP_CRITTER; /* This acts just like a critter, so doesn't 
			need any special priority.  In fact we want a low priority so regular bullets
			have higher priority and can kill it in their own collide calls.
			Don't use the setCollidePriority mutator, as that forces a call to pgame()->buildCollider(); */
            cSpriteSphere bulletsprite = new cSpriteSphere(cCritter.BULLETRADIUS, 6, 6);
			bulletsprite.FillColor = Color.Red; //From colornames.h, should make these static readonlys.
            Sprite = bulletsprite; /* Also sets cSprite._prismdz to CritterBullet.BULLETPRISMDZ. */ 
		}

        public override cCritterBullet Create()
        {
            return new cCritterBulletRubber();
        }
		
		public override bool isTarget( cCritter pcritter ){ return false; } // Don't blow up anyone.
		
		public override void update( ACView pactiveview, float dt ) //Don't use the cCritterBullet update.
		{
            _baseAccessControl = 1;
			base.update( pactiveview, dt ); //Don't do the bullet stuff.
            _baseAccessControl = 0;
        } 

		/* Overload to use the cCritter update instead of 
			the cCritterBullet update that you inherit. */ 
	
		public override bool collide( cCritter pcritter ) 
		{
            _baseAccessControl = 1;
            bool success = base.collide(pcritter);  // Regular collide.
            _baseAccessControl = 0;
            return success;
		} 

        public override bool IsKindOf( string str )
        {
            return str == "cCritterBulletRubber" || base.IsKindOf( str );
        }
		
        public override string RuntimeClass
		{
			get
            {
                return "cCritterBulletRubber";
            }
		}


	} 
	
	class cCritterBulletSilver : cCritterBullet 
	{ 
		public static readonly int SCOREVALUE = 10; 
		
		public cCritterBulletSilver() 
		{ 
			_value = cCritterBulletSilver.SCOREVALUE; 
			_collidepriority = cCollider.CP_SILVERBULLET; /* Want this to have a lower priority than
			CP_BULLET, so that bullets can kill it in their collide call. 
			Don't use the setCollidePriority mutator, as that forces a call to
			pgame()->buildCollider(); */
            cSpriteSphere bulletsprite = new cSpriteSphere(cCritter.BULLETRADIUS, 6, 6);
			bulletsprite.FillColor = Color.LightGreen; 
			Sprite = bulletsprite; /* Also sets cSprite._prismdz to CritterBullet.BULLETPRISMDZ. */ 
		}

        public override cCritterBullet Create()
        {
            return new cCritterBulletSilver();
        }
		
		public override bool isTarget( cCritter pcritter ) 
		{ 
			return pcritter == _ptarget; 
		} 

		// Only blows up _ptarget.
	
		public override int damage( int hitstrength ) 
		{
			Framework.snd.play( Sound.Stomp );
			return base.damage( hitstrength ); // will call cCritter damage
		} 

		//Make a noise and call cCritter.damage.

        public override bool IsKindOf( string str )
        {
            return str == "cCritterBulletSilver" || base.IsKindOf( str );
        }
	
        public override string RuntimeClass
		{
			get
			{
                return "cCritterBulletSilver";
            }
		}


	} 
	
	class cCritterBulletSilverMissile : cCritterBulletSilver 
	{ 
		public static readonly new int SCOREVALUE = 20; 
		public static readonly new float MAXSPEED = 10.0f; 
		public static readonly float CHASEACCELERATION = 20.0f; //Rate you accelerate towards player.
		
		public cCritterBulletSilverMissile() 
		{ 
			_value = cCritterBulletSilverMissile.SCOREVALUE; 
			_maxspeed = cCritterBulletSilverMissile.MAXSPEED; 
			_dieatedges = false; //Make them particularly vicious and long-lived.
            cSpriteSphere bulletsprite = new cSpriteSphere(cCritter.BULLETRADIUS, 6, 6);
			bulletsprite.FillColor = Color.LightBlue; 
			Sprite = bulletsprite; /* Also sets cSprite._prismdz to CritterBullet.BULLETPRISMDZ. */ 
		}

        public override cCritterBullet Create()
        {
            return new cCritterBulletSilverMissile();
        }
		
		public override void initialize( cCritterArmed pshooter ) 
		{ 
			base.initialize( pshooter );  // calls the cCritterBullet initialize 
			addForce( 
				new cForceObjectSeek( Target, cCritterBulletSilverMissile.CHASEACCELERATION ) 	); 
		} 

		//Overload to make a cForceObjectSeek 

        public override bool IsKindOf( string str )
        {
            return str == "cCritterBulletSilverMissile" || base.IsKindOf( str );
        }
	
        public override string RuntimeClass
		{
			get
			{
                return "cCritterBulletSilverMissile";
            }
		}


	} 
	
	class cCritterArmed : cCritter 
	{ 
		public static readonly float GUNLENGTH = 1.3f; //Ratio of gun length to critter radius, maybe 1.3.
		public static readonly float WAITSHOOT = 0.06f; // The default interval to wait between shots.
		protected bool _armed; //Use this to turn the gun on or off.
		protected float _ageshoot; //Age at last shot, so you wait a bit between shots 
		protected float _waitshoot; //Time to wait between shots.
		protected float _gunlength; 
		protected cVector3 _aimvector; 
		protected bool _bshooting; 
		protected bool _aimtoattitudelock; //FALSE means aim any old way, TRUE means match aim to attitude tangent 
		private cCritterBullet _pbulletclass; 
	//Nonserializable fields 
		protected LinkedList<cCritterBullet> _bulletlist; 
		
        public cCritterArmed( cGame pownergame = null ) 
            : base( pownergame )
        {
		    _armed = true; 
		    _aimvector = new cVector3( 0.0f, 1.0f ); 
		    _ageshoot = 0.0f; 
		    _waitshoot = WAITSHOOT; 
		    _bshooting = false; 
		    _aimtoattitudelock = false; 
		    _gunlength = GUNLENGTH;
            BulletClass = new cCritterBulletRubber();
			AimVector = _tangent;
            _bulletlist = new LinkedList<cCritterBullet> (
                delegate ( out cCritterBullet c1, cCritterBullet c2 )
                {
                    c1 = c2;
                }
                );
		} 

		
		public override void destruct() 
		{ 
		/* It could cause a crash if any surviving cCritterBullet still has a _pshooter pointer to
			this deleted cCritterArmed.  So I set all the bullets' pshooter to NULL, and everywhere in
			the bullet code where I might use a pshooter, I always check that it isn't NULL. */

            base.destruct();
            foreach ( cCritterBullet cb in _bulletlist )
                _bulletlist.ElementAt().nullTheShooter();
		} 
		
		public override void copy( cCritter pcritter ) 
		{ 
		/*We need to overload the cCritter copy because if I have cCritterArmedRobot which is
	a cCritterArmed, and it gets split in two by a replicate call, then I want the new
	copy to have all the same shooting behavior as the old one.  In general, I should
	overload copy for each of my critter child classes, but for now this one is the
	most important.  The overload does the regular copy, then looks if the thing 
	being copied is a cCritterArmed, and if it is then it copies the additional fields. */ 
			base.copy( pcritter ); 	
			if ( !pcritter.IsKindOf( "cCritterArmed" )) 
				return ; //You're done if pcritter isn't a cCritterArmed*.
			cCritterArmed pcritterarmed = ( cCritterArmed )( pcritter ); /* I know it is a
			cCritterArmed at this point, but I need to do a cast, so the compiler will let me
			call a bunch of cCritterArmed methods. */ 
			_armed = pcritterarmed._armed; 
			_aimvector = pcritterarmed._aimvector; 
			_waitshoot = pcritterarmed._waitshoot; 
			_aimtoattitudelock = pcritterarmed._aimtoattitudelock; 
			BulletClass = pcritterarmed._pbulletclass; 
		}
 
        public override cCritter copy( )
        {
            cCritterArmed c = new cCritterArmed();
            c.copy(this);
            return c;
        }

	//Accessors 
	
	//Mutators 
		
		public virtual void aimAt( cVector3 vclick ) 
		{ 
			if ( !_armed ) 
				return ; 
			AimVector = ( vclick.sub( _position )).Direction; /* Use the setAimVector call as this
			does a safety roundOff on the direction. */ 
			if ( _aimtoattitudelock ) 
				AttitudeTangent = _aimvector; 
		} 

		//aim on line from position towards vclick.
	
		public virtual void aimAt( cCritter pcritter ) 
		{ /* I could make this code more sophisticated by taking into account the current velocity and
			distance of the pcritter to figure out where it will be when the bullet gets there. */ 
			if ( _armed && (pcritter != null) ) 
				aimAt( pcritter.Position); 
		} 

		//Aim at position of pcritter.
	
		/* aim parallel to dir, note dir
				 doesn't have to be unit vector */ 
	
		public void removeBullet( cCritterBullet pbullet ) 
		{

            if (_bulletlist.Size == 0)
                return;
            
            cCritterBullet cb;
            _bulletlist.First(out cb);

            do
            {
                if (_bulletlist.ElementAt() == pbullet)
                {
                    _bulletlist.RemoveNext();
                    break;
                }
            } while (_bulletlist.GetNext(out cb));
		} 

		
	//Overloads 
		
		public override void update( ACView pactiveview, float dt ) 
		{ 
		//(1) Call base class update to apply force.
			base.update( pactiveview, dt ); 
		//(2) Align gun with move direction if necessary.
			if ( _aimtoattitudelock ) 
				AimVector = AttitudeTangent; /* Keep the gun pointed in the right direction. */ 
		//(3) Shoot if possible.
			if ( !_armed || !_bshooting ) 
				return ; 
			/* If _age has been reset to 0.0, you need to get ageshoot back in synch. */ 
			if ( _age < _ageshoot ) 
				_ageshoot = _age; 
			if (( _age - _ageshoot > _waitshoot )) //A shoot key is down 
			{
				shoot(); 
				_ageshoot = _age; 
			} 
		} 

        public override void draw( cGraphics pgraphics, int drawflags = 0 ) 
		{ 
			if ( _armed ) /* The gun looks bad if
			 you're near the edge in 2D	graphics as it doesn't get clipped. */ 
			{ 
		/* We draw the "gun" as a line.  It might be better to have the following
	code be part of the cSprite.drawing, because then that would happen while the
	pDC is clipped in the cCritter call.  One way to do it would be to have a CpopView.DF_GUN
	drawflag. */ 
				cVector3 start = _position; 
				cVector3 end = start.add( _aimvector.mult( _gunlength * Radius)); 
			} 
			base.draw( pgraphics, drawflags ); 
				//Draw sprite on top of gun line.
		} 

		
		public override void animate( float dt ) 
		{ 
			base.animate( dt ); //Calls updateAttitude(dt) and _psprite->animate(dt) 
			if ( _aimtoattitudelock ) 
				AimVector = AttitudeTangent.roundOff(); 
		} 

		//Overload for possibly locking gun to sprite direction.
	//Special Methods 
	
		public virtual cCritterBullet shoot() 
		{ 
            cCritterBullet pbullet = _pbulletclass.Create(); 
			pbullet.initialize( this ); 
			pbullet.Sprite.LineColor = Sprite.LineColor;
			pbullet.add_me( _pownerbiota ); /* Makes a servicerequest to be handled by cBiota later. I used to
				have _pownerbiota.Add(pbullet) here, but this makes a problem if I do
				USEMETRIC; this is because _metric expects the critter's indices to stay fixed.
				In general, I should not be adding or deleting any critters except
				in the cBiota.processervicerequests call. Note that you have the default
				FALSE value of the immediateadd argument to add_me, meaning you don't
				add in this critter to the simulator cBiota until it finishes its current
				update loop and has a chance to call processServiceRequests. */ 
			_bulletlist.Add( pbullet ); //Adds to end of my personal bullet-data array.
			return pbullet; /* In case you want to overload cCritterArmed.shoot to do something else to 
			the bullet. */ 
		} 

		/* Create a bullet and add it to your personal tracking
			array. We return the bullet in case we want to overload the shoot method do do
			something else to it. */ 

        public override bool IsKindOf( string str )
        {
            return str == "cCritterArmed" || base.IsKindOf( str );
        }
	
		public virtual cVector3 AimVector
		{
			get
			{ 
			    cVector3 a = new cVector3(); 
			    a.copy( _aimvector ); 
			    return a; 
		    }
			set
			{ 
			    cVector3 d = new cVector3(); 
			    d.copy( value ); 
			    d.roundOff(); /* I need this because in 3D with cGraphicsOpenGL, I will sometimes have
			    an erroneous tiny z value that makes the planer seem to aim out of his plane. */ 
			    _aimvector = d.Direction; 
		    }
		}

		public virtual cCritterBullet BulletClass
		{
			get
				{ return _pbulletclass.Create(); }
			set
            {
                if (value.IsKindOf("cCritterBulletSilver") && this.IsKindOf("cCritterArmedPlayer"))
                    MessageBox.Show("ERROR:  The player cannot use cCritterBulletSilver or one of its derived classes");
                if (this.IsKindOf("cCritterArmedRobot") && !(value.IsKindOf("cCritterBulletSilver")
                    || value.IsKindOf("cCritterBulletRubber")))
                MessageBox.Show("ERROR:  cCritterArmedRobot (or a class derived from it) must use cCritterBulletSilver or cCritterBulletRubber (or classes derived from them)");
                _pbulletclass = value.Create(); 
            }
		}

		public virtual LinkedList<cCritterBullet> BulletList
		{
			get
				{ return _bulletlist; }
		}

		public virtual float GunLength
		{
			get
				{ return _gunlength; }
			set
				{ _gunlength = value; }
		}

		public virtual bool Armed
		{
			get
				{ return _armed; }
			set
				{ _armed = value; }
		}

		public virtual float WaitShoot
		{
			set
				{ _waitshoot = value; }
		}

		public virtual bool Shooting
		{
			set
				{ _bshooting = value; }
		}

		public virtual bool AimToAttitudeLock
		{
			set
				{ _aimtoattitudelock = value; }
		}

        public override string RuntimeClass
		{
			get
			{
                return "cCritterArmed";
            }
		}


	} 
	
	class cCritterArmedRobot : cCritterArmed 
	{ 
		
        public cCritterArmedRobot( cGame pownergame = null ) 
            : base( pownergame ) 
		{ 
			_bshooting = true; //Assume enemy shoots whenever time is up.
			WaitShoot = _waitshoot; 
		} 

		
	//overloads 
	
		//randomize ageshoot so N robots not in synch 
	
		//randmoize ageshoot so N robots not in synch.
	
		public override void update( ACView pactiveview, float dt ) 
		{ 
			base.update( pactiveview, dt ); 	/* Do the basic cCritterArmed update to apply the forces,
			in _forcearray, which might include a cForceObjectSeek to approach the player
			and a cForceClassEvade to avoid the bullets. */ 
			aimAt( _ptarget ); // Aim at the _ptarget (does nothing if _armed FALSE or if _ptarget NULL).
		} 

        public override cCritter copy( )
        {
            cCritterArmedRobot c = new cCritterArmedRobot();
            c.copy(this);
            return c;
        }

        public override bool IsKindOf( string str )
        {
            return str == "cCritterArmedRobot" || base.IsKindOf( str );
        }
		
		public override float WaitShoot
		{
			set
			{ 
			    _waitshoot = value; 
			    _ageshoot = _age - Framework.randomOb.randomReal( 0.0f, _waitshoot ); 
			    /* Do this so they don't all shoot at once,	when you have several of them. */ 
		    }
		}

		public override float Age
		{
			set
			{ 
			    base.Age = value;   // will call cCritter setAge
			    WaitShoot = _waitshoot; 
		    }
		}

        public override string RuntimeClass
		{
			get
			{
                return "cCritterArmedRobot";
            }
		}


	} 
	
	class cCritterArmedPlayer : cCritterArmed 
	{ 
		public static readonly int PLAYERHEALTH = 10; 
		public static readonly int DAMAGESOUND = Sound.Stomp;
		public static readonly new float DENSITY = 5.0f;
        public static readonly float WAITSHOT = 0.5f;
		protected bool _sensitive; /* If TRUE, then you are damaged by a collision with 
			a cEnemyCritter */
        protected bool shotDone = true;
        protected bool timingAge = false;
        protected float startTime;

        public cCritterArmedPlayer( cGame pownergame = null ) 
            : base( pownergame )
        {
		    _sensitive = false; 
			_collidepriority = cCollider.CP_PLAYER; /* Don't use the setCollidePriority mutator, as that
			forces a call to pgame()->buildCollider(); */ 
		//	setDensity(cCritter.INFINITEDENSITY);  
				//So it can bull through.  But it's wiser to let the individual games do this.
			AttitudeToMotionLock = false; /* We want the player's
			attitude to be controlled by the listner actions and not by bumping into things,
			or moving with gravity. */ 
			AimToAttitudeLock = true; /* Normally
			we want a player to shoot in the dirction of his attitude. */ 
			cPolygon ppolygon = new cPolygon( 3 ); 
			/* Now make it a thin isoceles triangle, with the apex at the 0th vertex.
			All that matters at first is the ratios of the numbers, as we will use
			seRadius to make the thing the right size. */ 
			ppolygon.setVertex( 0, new cVector3( 4.0f, 0.0f )); 
			ppolygon.setVertex( 1, new cVector3( 0.0f, 1.0f )); 
			ppolygon.setVertex( 2, new cVector3( 0.0f, -1.0f )); 
			ppolygon.Radius = cCritter.PLAYERRADIUS; //Make it to a good size.
			Sprite = ppolygon; /* Normally the _prismdz will get changed to PLAYERPRISMDZ 
			by the cGame.setPlayer call */ 
			PrismDz = cSprite.PLAYERPRISMDZ; 
		} 

		
		public override void copy( cCritter pcritter ) 
		{ 
		/*We need to overload the cCritter copy because if I have cCritterArmedRobot which is
	a cCritterArmed, and it gets split in two by a replicate call, then I want the new
	copy to have all the same shooting behavior as the old one.  In general, I should
	overload copy for each of my critter child classes, but for now this one is the
	most important.  The overload does the regular copy, then looks if the thing 
	being copied is a cCritterArmed, and if it is then it copies the additional fields. */ 
			base.copy( pcritter ); 	
			if ( !pcritter.IsKindOf( "cCritterArmedPlayer" )) 
				return ; //You're done if pcritter isn't a cCritterArmed*.
			cCritterArmedPlayer pcritterplayer = ( cCritterArmedPlayer )( pcritter ); /* I know it is a
			cCritterArmed at this point, but I need to do a cast, so the compiler will let me
			call a bunch of cCritterArmed methods. */ 
			_sensitive = pcritterplayer._sensitive; 
		} 

        public override cCritter copy( )
        {
            cCritterArmedPlayer c = new cCritterArmedPlayer();
            c.copy(this);
            return c;
        }
		
	//Mutators 
	
		public override void reset() 
		{ 
			_lasthit_age = _age = 0.0f; 
			_health = PLAYERHEALTH; 
			_score = 0; 
			_position.setZero(); 
			_velocity.setZero(); 
			_acceleration.setZero(); 
			_spin.setZero(); 
			_speed = 0; 
		} 

		
	//Accessors 
		
	//Main methods 
		
		public override void feellistener( float dt ) 
		{ 
			base.feellistener( dt );  // will call cCritter feellistener

            if (!shotDone) // if space key or left mouse button is pressed, turn off shooting until not preesed
                _bshooting = false; 
            if (shotDone && ( Framework.keydev[vk.Space] || Framework.leftclick )) 
            { // if previous shot is done, turn on shooting when space key or left mouse button is pressed
                _bshooting = true;
                shotDone = false;
            }
            if (!shotDone && !timingAge && !Framework.keydev[vk.Space] && !Framework.leftclick)
            {
                // space key and mouse button are both lifted, so wait a little
                timingAge = true;
                startTime = _age;
            }

            if ( timingAge && (_age - startTime) > WAITSHOT )
                // if you don't wait long enough, sounds can be distorted (problem with OpenAL)
            {
                timingAge = false;
                shotDone = true;
            }

       } 

		/* Overload to listen for VK_SPACE to turn on _bshooting
	if you have a _plistener. */ 
	
		public override int damage( int hitstrength ) 
		{ 
			if (!( _shieldflag || recentlyDamaged())) 
			{ 
				Framework.snd.play( DAMAGESOUND ); 
		/* In some games we might want to recenter the player after damage, but for now let's not. */ 
		//		_position.setZero(); 
		//		_velocity.setZero(); 
			} 
			return base.damage( hitstrength ); // will call cCritter damage 
		} 

		/* ignores damage if _shieldflag on */ 
	
		public override bool collide( cCritter pcritter ) 
		{
            if (_baseAccessControl == 1)
                return base.collide(pcritter);
			bool collided = base.collide( pcritter );  // will call cCritter collide 
			if ( collided && _sensitive && !pcritter.IsKindOf( "cCritterWall" )) 
				damage( 1 ); 
			return collided; 
		} 

		//Can call damage or not.
	
        public override void draw( cGraphics pgraphics, int drawflags = 0 ) 
		{ 
			base.draw( pgraphics, drawflags ); 
		} 

		//Draw differently when _health == 0.
	
		public override cCritterBullet shoot() 
		{ 
			cCritterBullet pbullet = base.shoot(); 
		/* I used to just have 
		pbullet->addVelocity(_velocity), but this gives unattractive results. */ 
			float bulletspeedup = _velocity.mod( pbullet.Tangent ); 
			if ( bulletspeedup > 0.0f ) 
				pbullet.addVelocity( pbullet.Tangent.mult( bulletspeedup )); //So bullets don't stack up.
			return pbullet; 
		} 

		/* Call the standard shoot method but add your player
			velocity to the bullet because otherwise if you're flying forward the bullets pile up.*/ 

        public override bool IsKindOf( string str )
        {
            return str == "cCritterArmedPlayer" || base.IsKindOf( str );
        }
	
		public virtual bool Sensitive
		{
			get
				{ return _sensitive; }
			set
				{ _sensitive = value; }
		}

        public override string RuntimeClass
		{
			get
			{
                return "cCritterArmedPlayer";
            }
		}


	} 
	
	class cCritterPlayer : cCritterArmedPlayer 
	{ 
		
        public cCritterPlayer( cGame pownergame = null ) 
            : base( pownergame ) 
		{ 
			_bshooting = false; 
		}

        public override cCritter copy( )
        {
            cCritterPlayer p = new cCritterPlayer();
            p.copy(this);
            return p;
        }

        public override bool IsKindOf( string str )
        {
            return str == "cCritterPlayer" || base.IsKindOf( str );
        }
 
        public override string RuntimeClass
		{
			get
			{
                return "cCritterPlayer";
            }
		}
	}
}



                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           