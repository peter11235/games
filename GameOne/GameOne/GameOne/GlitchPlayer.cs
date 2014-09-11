/*
 * Represents a GlitchPlayer.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace GameOne
{
    class GlitchPlayer : UserControlledSprite
    {
        float gravity = 3;
        
        // state pattern
        const int numberStates = 7;
        enum GlitchPlayerState
        {
            Walking,
            Climbing,
            Jumping,
            Sleeping
        }
        GlitchPlayerState currentState;
        AbstractState[] states;

        public GlitchPlayer(Texture2D image)
             : base(new SpriteSheet(image, new Point(21, 6)), Vector2.Zero, 
            10, new Vector2(2, 2))
            
        {
            // set the segments
            Point frameSize = new Point(192, 160);
            spriteSheet.addSegment(frameSize, new Point(0, 0), new Point(11,0), 50);
            spriteSheet.addSegment(frameSize, new Point(0, 1), new Point(18, 1), 50);
            spriteSheet.addSegment(frameSize, new Point(0, 2), new Point(11, 3), 50);
            spriteSheet.addSegment(frameSize, new Point(0, 4), new Point(20, 5), 50);

            // define the states
            states = new AbstractState[numberStates];
            states[(Int32)GlitchPlayerState.Walking] = new WalkingState(this);
            states[(Int32)GlitchPlayerState.Sleeping] = new SleepingState(this);
            states[(Int32)GlitchPlayerState.Jumping] = new JumpingState(this);
            states[(Int32)GlitchPlayerState.Climbing] = new ClimbingState(this);

            // start in Walking state
            switchState(GlitchPlayerState.Walking);
            
            
        }

        public override void Update(GameTime gameTime, Rectangle clientBounds)
        {
            float bottom = clientBounds.Bottom;
            if (currentState == GlitchPlayerState.Walking || currentState == GlitchPlayerState.Jumping)
            {
                if (this.position.Y < bottom+15)
                {
                    this.position.Y += gravity;
                }
            }
            states[(Int32)currentState].Update(gameTime, clientBounds);
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A) || Keyboard.GetState().IsKeyDown(Keys.Space)) //jump
            {
                switchState(GlitchPlayerState.Jumping);
            }
            
            base.Update(gameTime, clientBounds);
        }

        private void switchState(GlitchPlayerState newState)
        {
            pauseAnimation = false;
            currentState = newState;
            spriteSheet.setCurrentSegment((Int32)newState);
            currentFrame = spriteSheet.currentSegment.startFrame;
        }


        /** STATES **/
        private abstract class AbstractState
        {
            protected readonly GlitchPlayer player;

            protected AbstractState(GlitchPlayer player)
            {
                this.player = player;
            }

            public virtual void Update(GameTime gameTime, Rectangle clientBounds)
            {
            }
        }
        
        /* Walking State */
        private class WalkingState : AbstractState
        {
            Point stillFrame;
            int timeSinceLastMove = 0;
            const int timeForSleep = 3000;

            public WalkingState(GlitchPlayer player)
                : base(player)
            {
                stillFrame = new Point(14, 0);
            }

            public override void Update(GameTime gameTime, Rectangle clientBounds)
            {
                // pause animation if the sprite is not moving
                if (player.direction.X == 0 && player.direction.Y == 0)
                {
                    player.pauseAnimation = true;
                    player.currentFrame = stillFrame; // standing frame
                }
                else
                {
                    timeSinceLastMove = 0;
                    player.pauseAnimation = false;
                }
                if (player.direction.X == 0 && player.direction.Y != 0)
                {
                    player.switchState(GlitchPlayerState.Climbing);
                }
                // transition to sleep state?
                timeSinceLastMove += gameTime.ElapsedGameTime.Milliseconds;
                if (timeSinceLastMove > timeForSleep)
                {
                    timeSinceLastMove = 0;
                    player.switchState(GlitchPlayerState.Sleeping);
                }
            }
        }

        /* Sleeping State */
        private class SleepingState : AbstractState
        {
            Vector2 sleepingPosition;
            Boolean fallingToSleep = true;

            public SleepingState(GlitchPlayer player)
                : base(player)
            {
            }

            public override void Update(GameTime gameTime, Rectangle clientBounds)
            {


                if (fallingToSleep)
                {
                    sleepingPosition = player.position;
                    fallingToSleep = false;
                }

                if (player.currentFrame == player.spriteSheet.currentSegment.endFrame)
                {
                    player.pauseAnimation = true;
                }

                if (sleepingPosition != player.position)
                {
                    fallingToSleep = true;
                    player.switchState(GlitchPlayerState.Walking);
                }
            }
        }


        /* Jumping State */
        private class JumpingState : AbstractState
        {
            Point halfway = new Point(15, 2);
            public JumpingState(GlitchPlayer player)
                : base(player)
            {
            }

            public override void Update(GameTime gameTime, Rectangle clientBounds)
            {

                player.pauseAnimation = false;
                player.position.X += 2*player.direction.X;

                if (player.currentFrame.X < 15 && player.currentFrame.Y == 2)
                {
                    player.position.Y -= 5;
                }

                if (player.currentFrame == player.spriteSheet.currentSegment.endFrame)
                {
                    player.switchState(GlitchPlayerState.Walking); //start walking
                }
            }
        }

        /* Climbing State */
        private class ClimbingState : AbstractState
        {
            Point startFrame;
            public ClimbingState(GlitchPlayer player)
                : base(player)
            {
                startFrame = new Point(0, 1);
            }

            public override void Update(GameTime gameTime, Rectangle clientBounds)
            {
                player.pauseAnimation = false;

                if (player.direction.X == 0 && player.direction.Y == 0)
                {
                    player.pauseAnimation = true;
                }
                if (player.direction.X != 0) //if player starts moving horizontally
                {
                    player.switchState(GlitchPlayerState.Walking);
                }
                if (player.currentFrame == player.spriteSheet.currentSegment.endFrame)
                {
                    player.currentFrame = startFrame; //keep climbing until X position changes
                }
            }
        }
    }
}
