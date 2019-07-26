using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Penguin_Script : MonoBehaviour
{
    /* difficulty settings -
     * 
     * penguin_switch_num: this is the minimum number of penguins left in the game 
     * where switching to track two is allowed, when it is less than this, once a 
     * penguin is on track one it will never go to track two again
     */
    public int penguin_switch_num;

    //variables for objects in scene
    public Animator animator;
    public Rigidbody penguin;
    public Rigidbody player;
    public Collider penguin_collider;

    //numerical, boolean, & string variables for penguin logic
    public float max_velocity;
    public bool has_collided_with_player;
    public float time_on_ground;
    public string direction;
    public bool is_in_pen;
    public bool sliding_back;
    public float timer;
    public int cur_num_penguins;

    //scripts
    public Player_Script player_script;
    public Game_Runner game_runner_script;

    /*
     * Start is called before the first frame update
     */
    void Start()
    {
        //initializes all of the variables that are used during execution
        player_script = player.GetComponent<Player_Script>();
        penguin = GetComponent<Rigidbody>();
        game_runner_script = FindObjectOfType<Game_Runner>();
        animator = gameObject.GetComponent<Animator>();
        penguin_collider = penguin.GetComponent<Collider>();

        penguin.useGravity = true;
        penguin.isKinematic = false;

        max_velocity = 4.0f;
        has_collided_with_player = false;
        direction = "";
        is_in_pen = false;
        sliding_back = false;
        penguin_switch_num = 4;

        //randomly decides which direction the penguin is facing at start
        int rand_walk = Random.Range(0, 1);
        if (rand_walk == 0)
        {
            animator.Play("walk_left");
        }
        else
        {
            animator.Play("walk_right");
        }
    }

    /*
     * Update is called every frame
     */
    void Update()
    {
        //current number of penguins outside the enclosure
        cur_num_penguins = game_runner_script.Get_Num_Penguins();

        //this is used to check which direction penguin going
        var local_velocity = transform.InverseTransformDirection(penguin.velocity);

        //if the penguin is currently sliding back toward the igloo (back of enclosure)
        if (sliding_back)
        {
            //this x value is a little in front of the igloo to the edge of screen view
            float rand_x = Random.Range(3.5f, 6.0f);
            
            /*
             * the destination the penguin Lerps toward is on a random x from earlier and on a track aligned 
             * with the entrance of the igloo (z = 8.2f)
             */ 
            Vector3 destination = new Vector3(rand_x, 0.0f, 8.2f);

            //the penguin slides back, while Lerping toward the destination
            animator.Play("run_back");
            animator.SetBool("run_back_bool", true);
            penguin.transform.position = Vector3.Lerp(penguin.position, destination, 0.075f);

            //once the penguin gets past the fence, it is no longer sliding back
            if (penguin.position.z > 8.0f)
            {
                sliding_back = false;
            }
        }
        else
        {
            //this means the penguin has stopped bouncing
            if (penguin.velocity.y == 0.0f)
            {
                if (direction == "left")
                {
                    //the penguin is currently facing left, so it will be idle left
                    animator.SetBool("idle_left_bool", true);
                    time_on_ground++;
                }
                else if (direction == "right")
                {
                    //the penguin is currently facing right, so it will be idle right
                    animator.SetBool("idle_right_bool", true);
                    time_on_ground++;
                }

                //if the penguin has been idle more than 160 frames, it will bounce again
                if (time_on_ground > 160)
                {
                    //time on ground reset to zero
                    time_on_ground = 0;
                    if (direction == "left")
                    {
                        //will bounce left since it's facing left
                        StartCoroutine(DelayJumpAfterRun());
                    }
                    else if (direction == "right")
                    {
                        //will bounce right since it's facing right
                        StartCoroutine(DelayJumpAfterRun());
                    }
                }
            }
            //this means the penguin is in motion
            else
            {
                if (local_velocity.x < 0)
                {
                    //the penguin is currently moving left
                    direction = "left";
                    ResetAnimations();
                    animator.SetBool("walk_left_bool", true);
                }
                else
                {
                    //the penguin is currently moving right
                    direction = "right";
                    ResetAnimations();
                    animator.SetBool("walk_right_bool", true);
                }
            }

            /*
             * this check ensures that the velocity of the penguin never exceeds the maximum so that 
             * the bouncing never becomes too extreme
             */
            if (penguin.velocity.magnitude > max_velocity)
            {
                penguin.velocity = max_velocity * penguin.velocity.normalized;
            }

            /*
             * if a penguin goes too far on the left, a force is added to it to 
             * bounce it back towards the play area to ensure a penguin never goes off-screen.
             */
            if (penguin.position.x < -12.0f)
            {
                //too far left
                if (penguin.position.x < -12.0f)
                {
                    Bounce(200.0f, 50.0f, 0.0f);
                }
            }
        }
    }

    /*
     * this function allows the penguin to jump after performing a slide (run animation)
     */
    public IEnumerator DelayJumpAfterRun()
    {
        /* the penguin slides left/right, 
         * then waits 0.05 seconds before walking
         * again so the slide can complete, 
         * then does a bounce to continue motion
         */
        if (direction == "left")
        {
            animator.Play("run_left");
            Bounce(-900.0f, 0.0f, 0.0f);
            yield return new WaitForSeconds(0.05f);
            ResetAnimations();
            animator.SetBool("walk_left_bool", true);
        }
        else
        {
            animator.Play("run_right");
            Bounce(900.0f, 0.0f, 0.0f);
            yield return new WaitForSeconds(0.05f);
            ResetAnimations();
            animator.SetBool("walk_right_bool", true);
        }

        Bounce(0.0f, 850.0f, 0.0f);
    }

    /*
     * this method is called whenever a penguin collides with an object
     */
    public void OnCollisionEnter(Collision collision)
    {
        /* Collision with the exhibit entrance
         * there is an "invisible" wall that separates the penguins from the snow area,
         * and is only deactivated once the penguin collides with the player, this 
         * ensures penguins will not enter the igloo before gameplay has begun
         */ 
        if (collision.gameObject.name == "Exhibit_Entrance")
        {
            if (has_collided_with_player && !is_in_pen)
            {
                //allows the penguin to enter the snow area because the collision with the invisible wall will be ignored
                Physics.IgnoreCollision(collision.collider, this.penguin_collider, true);
            }
            else
            {
                //the wall bounces the penguin back
                Bounce(-300.0f, 4.0f, 0.0f);
            }
        }

        /* Collision with the left wall:
         * there is an invisible wall on the left of the playing area so that penguins
         * never leave the scene
         */ 
        if(collision.gameObject.name == "Wall_Left")
        {
            /* penguin is moved to the third track so that it can bounce back without being interrupted by
             * the player
             */
            penguin.transform.position = new Vector3(transform.position.x + 2.0f, transform.position.y, -3.0f);
            Bounce(300.0f, 4.0f, 0.0f);
        }

        /* Collision with the right wall:
         * there is an invisible wall on the right of the playing area so that penguins
         * never leave the scene
         */
        if (collision.gameObject.name == "Wall_Right")
        {
            /* penguin is moved to the third track so that it can bounce back without being interrupted by
             * the player
             */
            penguin.transform.position = new Vector3(transform.position.x - 2.0f, transform.position.y, -3.0f);
            Bounce(-300.0f, 4.0f, 0.0f);
        }

        /* Collision with the player:
         */
        if (collision.gameObject.name == "Player")
        {
            //sets flag and allows penguin to enter the exhibit area
            has_collided_with_player = true;
            //bounces the penguin with a substantial amount of force when colliding with the user, can adjust as necessary
            Bounce(500.0f, 5.0f, 0.0f);
        }

        /* Collision with igloo hole:
         */ 
        if (collision.gameObject.name == "Igloo_Hole")
        {
            //once the penguin enters the igloo it is destroyed
            Destroy(this.gameObject);
        }

        /* Collision with the back of the snow area:
         * this collision is registered when the penguin bounces in the back area of the snow enclosure,
         * it is different from the rest of the snow because the behavior for the tracks is different
         */ 
        if (collision.gameObject.name == "Snow_Back")
        {
            //if the penguin is right in front of the igloo hole, it slides into it
            if(penguin.position.z == 8.2f && penguin.position.x < 3.0f)
            {
                Bounce(-4000, 0.0f, 0.0f);
                animator.Play("run_left");
            }

            //variables for the current position of the penguin
            float cur_x = penguin.position.x;
            float cur_y = penguin.position.y;
            float cur_z = penguin.position.z;

            //the new value for the y of the penguin
            float rand_y;
            rand_y = Random.Range(35.0f, 65.0f);
            int rand = RandomNum();

            //30% of the time, the penguin will attempt to switch tracks
            if (rand <= 3)
            {
                /* if the penguin is on the track not aligned with the igloo, and it is in front of the igloo,
                 * it can move to the track aligned with the igloo
                 */ 
                if(penguin.position.z == 10.8f)
                {
                    if (penguin.position.x > 3.5f)
                    {
                        penguin.transform.position = new Vector3(cur_x, cur_y, 8.2f);
                    }
                }
                /* otherwise, it moves to the track behind the igloo
                 */ 
                else
                {
                    penguin.transform.position = new Vector3(cur_x, cur_y, 10.8f);
                }
            }

            //50% of the time, the penguin will bounce to the right
            if (RandomNum() <= 5)
            {
                //positive x value means bounce to the right
                Bounce(300.0f, rand_y, 0.0f);
            }
            //50% of the time, the penguin will bounce to the left
            else
            {
                //negative x value means bounce to the left
                Bounce(-300.0f, rand_y, 0.0f);
            }
        }

        /* Collision with front snow area:
         * this collision is registered when the penguin bounces on the snow area directly to the right
         * of the grass, after a player has pushed it there. the penguin will either slide back 
         * into the enclosure, or slide left back outside onto the grass and sidewalk
         */ 
        if (collision.gameObject.name == "Snow_Front")
        {
            //penguin is now in pen
            is_in_pen = true;

            //if the penguin is not currently sliding back
            if (!sliding_back)
            {
                //random number to determine what the penguin will do, sometime it slides back, sometimes it slides left
                int rand_snow = RandomNum();

                /* 30% of the time OR if there are less than 2 penguins outside the pen, it will slide back. the 
                 * condition ensuring it's x-position is greater than 2.5 is to ensure it doesn't Lerp into the
                 * fence
                 */ 
                if ((rand_snow <= 3 || game_runner_script.Get_Num_Penguins() < 2) && penguin.position.x >= 2.5f)
                {
                    //sliding back into the enclosure
                    sliding_back = true;
                    //number of penguins outside the enclosure is decremented
                    game_runner_script.Decrement_Num_Penguins();
                }
                else
                {
                    //penguin slides to the left back into the enclosure
                    ResetAnimations();
                    animator.Play("run_left");
                    animator.SetBool("run_left_bool", true);
                    Bounce(-20000.0f, 5.0f, 0.0f);
                }
            }
        }

        /* Collision with the grass/sidewalk area:
         * 
         * penguins move between track one, two and three randomly upon colliding with the ground.
         * 
         * 
         * IMPORTANT: only when they are on track one or two are they aligned properly to be affected 
         * by the user
         * 
         * from the view of the player, the tracks are basically the same, since their view is "2D", 
         * so it simply appears that the penguins sometimes are pushed and sometimes aren't
         * 
         * track one: along the line z = -7.0f
         * track two: along the line z = -5.0f
         * track three: along the line z = -3.0f
         */
        if (collision.gameObject.tag == "Ground_Tag")
        {
            //not in the pen if colliding on the grass/sidewalk
            is_in_pen = false;

            //sets a random number from 1 (inclusive) to 10 (inclusive)
            int rand = RandomNum();

            //variables for the current position of the penguin
            float cur_x = penguin.position.x;
            float cur_y = penguin.position.y;
            float cur_z = penguin.position.z;

            if (OnTrackOne())
            {
                //50% of the time, the penguin will switch to track two
                if (rand <= 5)
                {
                    penguin.transform.position = new Vector3(cur_x, cur_y, -5.0f);
                }
            }
            else if (OnTrackTwo())
            {
                //30% of the time, the penguin will switch to track one
                if (rand <= 3)
                {
                    penguin.transform.position = new Vector3(cur_x, cur_y, -7.0f);
                }
                //30% of the time, the penguin will try to switch to track three
                else if (rand <= 7)
                {
                    //will switch to track three if there are more than the specified number of penguins outside still
                    if (cur_num_penguins > penguin_switch_num)
                    {
                        penguin.transform.position = new Vector3(cur_x, cur_y, -3.0f);
                    }
                }
            }
            else if (OnTrackThree())
            {
                //50% of the time, or if there are less than 3 penguins, the penguin will 
                //switch to track two
                if (rand <= 5 || cur_num_penguins <= 3)
                {
                    penguin.transform.position = new Vector3(cur_x, cur_y, -5.0f);
                }
            }

            //the new value for the y of the penguin
            float rand_y = Random.Range(35.0f, 65.0f);

            //50% of the time, the penguin will bounce to the right
            if (RandomNum() <= 5)
            {
                //positive x value means bounce to the right
                Bounce(300.0f, rand_y, 0.0f);
            }
            //50% of the time, the penguin will bounce to the left
            else
            {
                //negative x value means bounce to the left
                Bounce(-300.0f, rand_y, 0.0f);
            }
        }

        /* Collision with another penguin:
         */ 
        if (collision.gameObject.tag == "Penguin_Tag")
        {
            //50% penguin will bounce left, 50% will bounce right
            int rand_num = RandomNum();
            if (rand_num <= 5)
            {
                Bounce(400.0f, 25.0f, 0.0f);
            }
            else
            {
                Bounce(-400.0f, 25.0f, 0.0f);
            }
        }
    }

    /*
     * this function allows the penguin to slide back toward the igloo
     */
    public IEnumerator DelayAfterRunBack()
    {
        penguin.constraints = RigidbodyConstraints.None;
        penguin.constraints = RigidbodyConstraints.FreezeRotation;
        animator.Play("run_back");
        Bounce(0.0f, 0.0f, 1000.0f);
        yield return new WaitForSeconds(0.5f);
        ResetAnimations();
        animator.SetBool("walk_back_bool", true);
        animator.Play("walk_back");
        Bounce(0.0f, 2000.0f, 0.0f);
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
        animator.SetBool("walk_left_bool", false);
        animator.SetBool("walk_right_bool", false);
        animator.SetBool("walk_back_bool", false);
        animator.SetBool("run_left_bool", false);
        animator.SetBool("run_right_bool", false);
        animator.SetBool("run_back_bool", false);
        animator.SetBool("idle_left_bool", false);
        animator.SetBool("idle_right_bool", false);

        return;
    }

    /*
     * Adds a force to the penguin based on the x, y, and z parameters that 
     * allows it to jump
     * 
     * @param x x-value
     * @param y y-value
     * @param z z-value
     */
    public void Bounce(float x, float y, float z)
    {
        penguin.AddForce(x, y, z);
    }

    /*
     * Checks which track the penguin is currently on and returns a boolean
     * based on that
     * 
     * @return whether the penguin is on track one or not
     */
    public bool OnTrackOne()
    {
        if (penguin.position.z == -7.0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /*
     * Checks which track the penguin is currently on and returns a boolean
     * based on that
     * 
     * @return whether the penguin is on track two or not
     */
    public bool OnTrackTwo()
    {
        if (penguin.position.z == -5.0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /*
     * Checks which track the penguin is currently on and returns a boolean
     * based on that
     * 
     * @return whether the penguin is on track three or not
     */
    public bool OnTrackThree()
    {
        if (penguin.position.z == -3.0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
