using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Chicken_Script : MonoBehaviour
{
    //difficulty settings

    /* ramp_num: the higher the number, the more likely the chicken will bounce to
     * the henhouse (ex: 4 = 40% of the time)
     */
    public int ramp_num;
    
    /* chicken_switch_num: this is the minimum number of chickens left in the game 
     * where switching to track two is allowed, when it is less than this, once a 
     * chicken is on track one it will never go to track two again
     */
    public int chicken_switch_num;

    public Animation anim;
    public Animator animator;
    public Animation girl_anim;

    public Rigidbody chicken;
    public Rigidbody player;
    public GameObject girl;
    public Collider chicken_collider;

    public float max_velocity;
    public bool has_collided_with_player;
    public int num_switches;
    public float time_on_ground;

    public Player_Script player_script;
    public Girl_Script girl_script;
    public Game_Runner game_runner_script;

    public bool onTrack1;

    // Start is called before the first frame update
    void Start()
    {
        //initializes all of the variables that are used during execution
        player_script = player.GetComponent<Player_Script>();
        girl_script = girl.GetComponent<Girl_Script>(); 

        chicken = GetComponent<Rigidbody>(); 
        anim = gameObject.GetComponent<Animation>();
        girl_anim = girl.GetComponent<Animation>();
        game_runner_script = FindObjectOfType<Game_Runner>();
        animator = gameObject.GetComponent<Animator>();
        chicken_collider = chicken.GetComponent<Collider>();

        chicken.useGravity = true;
        chicken.isKinematic = false;

        max_velocity = 10.0f;
        has_collided_with_player = false;
        onTrack1 = true;
    }

    /*
     * called every frame
     */
    void Update()
    {
        /*
         * this check ensures that the velocity of the chicken never exceeds the maximum so that 
         * the bouncing never becomes too extreme
         */
        if (chicken.velocity.magnitude > max_velocity)
        {
            chicken.velocity = max_velocity * chicken.velocity.normalized;
        }

        /*
         * if the chicken is positioned on or a little in front of the ramp, it will play the
         * panic animation which also changes the rotation of the chicken to face the henhouse,
         * this will hopefully cue the user that the chicken is correctly positioned to enter
         * the henhouse
         */
        if (chicken.position.x > 0.0 && OnTrackOne())
        {
            ResetAnimations();
            animator.SetBool("panic_bool", true);
        }
        else
        {
            animator.SetBool("panic_bool", false);
            animator.SetBool("flyagain_bool", true);
        }

        /*
         * if a chicken goes too far on either side of the screen, a force is added to it to 
         * bounce it back towards the play area to ensure a chicken never goes offscreen.
         */
        if (chicken.position.x > 6.5f || chicken.position.x < -7.5f)
        {
            //too far right
            if (chicken.position.x > 6.5f && OnTrackTwo())
            {
                Bounce(-300.0f, 75.0f, 0.0f);
            }

            //too far left
            else if (chicken.position.x < -7.5f)
            {
                Bounce(100.0f, 50.0f, 0.0f);
            }
        }

        //make sure the chickens aren't stuck 
       // if (chicken.position.y > 20)
       if (this.name != "rudy" && !game_runner_script.isInBounds(chicken.position))
        {
            chicken.position =  new Vector3(0, 15, chicken.position.z);
            chicken.velocity = new Vector3(0, 0, 0);
        }
    }

    /*
     * this method is called whenever a chicken collides with an object
     */
    public void OnCollisionEnter(Collision collision)
    {

        //if the chicken collides with the player
        if (collision.gameObject.tag == "Player")
        {
            has_collided_with_player = true; //sets flag and allows chicken to enter henhouse

            /*
             * only plays the animation 'shout' if the chicken is not in the location where it's
             * oriented to enter the henhouse (i.e. if it's not currently playing 'panic', thus 
             * not altering its new rotation
             */
            if (!(chicken.position.x > 0.0) && !OnTrackOne())
            {
                ResetAnimations();
                animator.SetBool("shout_bool", true);
            }

            Bounce(2500.0f, 1000.0f, 0.0f); //bounces the chicken with a substantial amount of force when colliding with the user
        }

        //if the chicken collides with the grass
        if ((collision.gameObject.name == "Grass") || (collision.gameObject.tag == "Player"))
        {
            /*
             * chickens move between track one and track two randomly upon colliding with the ground, and 
             * only when they are on track one are they aligned properly to move into the henhouse.
             * 
             * from the view of the player, the tracks are the same, since their view is "2D", 
             * so it simply appears that the chickens sometimes are pushed right past the ramp
             * and sometimes they walk up it
             * 
             * track one: along the line z = -3.0f
             * track two: along the line z = -5.0f
             */

            //sets a random number from 1 (inclusive) to 10 (inclusive)
            int rand = RandomNum();

            //variables for the current position of the chicken
            float cur_x = chicken.position.x;
            float cur_y = chicken.position.y;
            float cur_z = chicken.position.z;

            //current number of chickens in game
            int cur_num_chickens = game_runner_script.Get_Num_Chickens();

            /* 
             * if chicken is on track two and is not further right than where the ramp starts,
             * this ensures that the chicken will not switch to track two and then get stuck under the ramp
             */
            if (OnTrackTwo())// && cur_x <= 3.0f)
            {
                //if there are fewer than 6 chicekns it will go to track one, otherwise 50% of the time chicken will switch
                if (cur_num_chickens < 6 || rand <= 5)
                {
                    //chicken switches to track one
                    // chicken.transform.position = new Vector3(cur_x, cur_y, -3.0f);
                    onTrack1=true;
                }
            }

            /* 
             * if chicken is on track one and there are more than four chickens currently in the game, this 
             * ensures that when the game is close to ending the chickens will mostly be oriented to
             * enter the henhouse
             */
            if (OnTrackOne())
            {
                //if there are more than 6 chickens it could switch 50% of the time
                if (cur_num_chickens > chicken_switch_num && rand <= 1)
                {
                    //chicken switches to track two
                    //chicken.transform.position = new Vector3(cur_x, cur_y, -5.0f);
                    onTrack1 = false;
                }

            }

            /*
             * changes the animation of the chicken with each collision with the ground, unless
             * the chicken is oriented to enter the henhouse, there's a 50% it will play 'pokpok'
             * and a 50% it will play 'cheer'
             */
            if (!animator.GetBool("panic_bool"))
            {
                ResetAnimations();
                if (rand <= 5)
                {
                    animator.SetBool("pokpok_bool", true);
                }
                else
                {
                    animator.SetBool("cheer_bool", true);
                }
            }

            float rand_y; //the new value for the y of the chicken

            int bounce_rand = RandomNum(); //random num for bounce from 1 (inclusive) to 10 (inclusive)

            //90% of the time, the chicken will bounce at a smaller interval, between 50 and 250
            if (bounce_rand <= 9)
            {
                rand_y = Random.Range(250.0f, 400.0f);
            }
            //10% of the time, the chicken will bounce at a larger interval, between 250 and 600
            else
            {
                rand_y = Random.Range(400.0f, 800.0f);
            }

            //50% of the time, the chicken will bounce to the right
            if (rand <= 5)
            {
                //positive x value means bounce to the right
                Bounce(500.0f, rand_y, 0.0f);
            }
            //50% of the time, the chicken will bounce to the left
            else
            {
                //negative x value means bounce to the left
                Bounce(-500.0f, rand_y, 0.0f);
            }
        }

        //if the chicken collides with the ramp
        if (collision.gameObject.name == "Ramp")
        {
           // Debug.Log("here");
            int rand_enter_num = RandomNum(); //random num for entering or not from 1 (inclusive) to 10 (inclusive)

            //chicken will bounce toward the henhouse based on the ramp_num
            if (rand_enter_num <= ramp_num)
            {
                Bounce(1800.0f, 750.0f, 0.0f);
            }
            //60% of the time, chicken will bounce away from the henhouse
            else
            {
                Bounce(-800.0f, 450.0f, 0.0f);
            }
        }

        //if the chicken collides with the henhouse
        if (collision.gameObject.tag == "Henhouse" && OnTrackOne())
        {
            /*
             * only if the chicken has collided with the player already is is able to enter the henhouse
             * and be destroyed, this ensures that the chickens will not enter the henhouse until
             * gameplay has started, and thus gives the actors ample time to set up the scene
             */
            if (has_collided_with_player)
            {
                //destroys the chicken
                Destroy(this.gameObject);

                //decrements the number of chickens in scene
               // game_runner_script.Decrement_Num_Chickens();

                //girl jumps in background unless the animation of her jumping is already currently playing
                girl_anim["Girl_Happy_Jump"].speed = 2.5f;
                if (!girl_anim.IsPlaying("Girl_Happy_Jump"))
                {
                    girl_anim.Play("Girl_Happy_Jump");
                }
            }
        }
    }


    /*
     * Generates a random number from 1 (inclusive) to 10 (inclusive)
     *
     * @return random integer
     */
    public int RandomNum()
    {
        int rand = Random.Range(1, 10);
        return rand;
    }

    /*
     * Resets the boolean values for all of the animations and thus causes
     * the animation controller to return to its default state
     */ 
    public void ResetAnimations()
    {
        animator.SetBool("panic_bool", false);
        animator.SetBool("flyagain_bool", false);
        animator.SetBool("shout_bool", false);
        animator.SetBool("panic_bool", false);
        animator.SetBool("cheer_bool", false);
        animator.SetBool("pokpok_bool", false);
        animator.SetBool("gethit_bool", false);
        //animator.SetBool("dizzy_bool", false);
        animator.SetBool("walk_bool", false);

        return;
    }

    /*
     * Adds a force to the chicken based on the x, y, and z parameters that 
     * allows it to jump
     * 
     * @param x x-value
     * @param y y-value
     * @param z z-value
     */ 
    public void Bounce(float x, float y, float z)
    {
        chicken.AddForce(x, y, z);
    }

    /*
     * Checks which track the chicken is currently on and returns a boolean
     * based on that
     * 
     * @return whether the chicken is on track one or not
     */ 
    public bool OnTrackOne()
    {
        //return true;
        return onTrack1;
        /*
        if (chicken.position.z == -3.0)
        {
            return true;
        }
        else
        {
            return false;
        }
        */
    }

    /*
     * Checks which track the chicken is currently on and returns a boolean
     * based on that
     * 
     * @return whether the chicken is on track two or not
     */
    public bool OnTrackTwo()
    {
        return !onTrack1;
        /*
        if (chicken.position.z == -5.0)
        {
            return true;
        }
        else
        {
            return false;
        }
        */
    }
}
