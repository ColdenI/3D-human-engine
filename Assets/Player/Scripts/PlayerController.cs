using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public float SpeedWalk = 6f;    // скорость ходьбы
    public float SpeedRun = 10f;    // скорость бега
    public float SpeedSlink = 3f;   // скорость красться
    public float SpeedCrawl = 2f;   // скорость ползти
    [Space(5)]
    public float JumpForce = 3f;    // сила прыжка
    [Space(5)]
    public float HeightFull = 2f;
    public float HeightSlink = 1f;
    public float HeightCrawl = .5f;
    [Space(5)]
    public float SpeedRecoveryHealth = .01f;
    [Space(5)]
    public float SpeedRecoveryStamina = .01f;
    public float SpeedRecoveryStamina_MultiplierRun = 2f;
    public float SpeedRecoveryStamina_MultiplierWalk = 1f;
    public float SpeedRecoveryStamina_MultiplierSlink = .7f;
    public float SpeedRecoveryStamina_MultiplierCrawl = .5f;
    [Space(5)]
    public float SpeedSpendingStamina = .02f;
    public float SpeedSpendingHunger = .01f;
    public float SpeedSpendingThirst = .01f;


    [Header("Settings")]
    public bool isShakingCamera = true;    //тряска камеры
    public float MouseSensitivity = 7f;    //чувствительность мыши
    public float FOV_Camera = 60f;
    [Space(5)]
    public KeyCode Jump_KeyControl = KeyCode.Space;
    public KeyCode Run_KeyControl = KeyCode.Tab;
    public KeyCode Slink_KeyControl = KeyCode.LeftShift;
    public KeyCode Crawl_KeyControl = KeyCode.LeftControl;
    public KeyCode Interaction_KeyControl = KeyCode.F;
    [Space(5)]
    public float rangeOfInteraction = 5f; // дальность взаимодействия


    [Header("Other Settings")]

    public float gravity = 9.8f;
    public float damageOfOctHeadbutt = 10f; // урон от удара головой
    public float dropDamageMultiplier = 3f; // множитель урона от падения
    public float minimumHungerForRecoveryHealthAndStamina = 30f; // минимальный порог для восстановления здоровья
    public float minimumThirstForRecoveryHealthAndStamina = 30f; // минимальный порог для восстановления выносливости
    public float minimumDropRateToTakeDamage = -10f; // минимальная скорость падения для получения урона
    public float delayReatartInteraction = .3f; // задержка перезарядки взаимодействия


    [Header("Sound")]
    public bool UsingSounds = true;
    [Space(5)]
    [SerializeField] private AudioSource Dialog_Source; // диалоги
    [SerializeField] private AudioSource Damage_Source; // получение урона
    [SerializeField] private AudioSource Medication_Source; // лечение
    [SerializeField] private AudioSource Eating_Source; // поедание
    [SerializeField] private AudioSource Drinking_Source; // питьё
    [SerializeField] private AudioSource Walking_Source; // хлдьба
    [SerializeField] private AudioSource Fatigue_Source; // усталость
    [SerializeField] private AudioSource Jumping_Source; // прыжок
    [Space(10)]
    [SerializeField] private AudioClip[] RandomDialog_Sounds;
    [SerializeField] private AudioClip[] TakeDamage_Sounds;
    [SerializeField] private AudioClip[] Medication_Sounds;
    [SerializeField] private AudioClip[] Eating_Sounds;
    [SerializeField] private AudioClip[] Drinking_Sounds;
    [SerializeField] private AudioClip Walking_Sound;
    [SerializeField][Range(0f, 3f)] private float walking_SoundMinSpeed = .5f;
    [SerializeField][Range(0f, 3f)] private float walking_SoundMaxSpeed = 1.5f;
    [SerializeField] private AudioClip[] Jumping_Sounds;
    [SerializeField] private AudioClip[] Fatigue_Sounds;
    [SerializeField] private AudioClip Death_Sound;


    [Header("Objects")]
    public GameObject MainCamera;
    public Animator HeadAnimation;

    [Space(10)]
    private CharacterController controller;
    private Animator playerAnimation;
    private Vector3 velocity;
    private Vector3 movement = Vector3.zero;
    private Vector3 moveJump_r;
    private Vector3 moveJump_f;
    private bool isGrounded;
    private bool isRuning = false;
    private bool isFellAtHighSpeed = false; // упал с высокой скоростью?
    private float isFellAtHighSpeed_lastVelocity; // упал с высокой скоростью? последняя скорость
    private bool isLockedRunFlag = true;
    private bool isThereIsObjectAboveHead = false; // над головой есть объект
    private bool FlagDelayInteraction = true;
    private Vector3 screenCenter;
    private bool isLockedMovement = false; // заблокировано передвижение
    private bool isLockedMouseMove = false; // заблокировано вращение камерой
    public bool IsLockedMovement { get => isLockedMovement; 
        set 
        { 
            isLockedMovement = value;
            if (isLockedMovement)
            {
                Walking_Source.Stop();
                isRuning = false;
            }
        } 
    }
    public bool IsLockedMouseMove { get => isLockedMouseMove; 
        set 
        { 
            isLockedMouseMove = value;
            _CameraMouseLock.Locked = isLockedMouseMove;
            _PlayerMouseLock.Locked = isLockedMouseMove;
        } 
    }

    private MouseLook _CameraMouseLock;
    private MouseLook _PlayerMouseLock;

    private float speed;
    [SerializeField] private float health = 100f; // здоровье 0-100
    [SerializeField] private float stamina= 100f; // выносливость 0-100
    [SerializeField] private float hunger = 100f; // голод 0-100
    [SerializeField] private float thirst = 100f; // жажда 0-100
    public float Health { get => health; }
    public float Stamina { get => stamina; }
    public float Hunger { get => hunger; }
    public float Thirst { get => thirst; }

    public delegate void OnPlayerInteracts(RaycastHit obj, PlayerController senderPlayer); // делегат для события взаимодействия
    public static event OnPlayerInteracts onPlayerInteracts; // событие при взаимодействии
    public delegate void OnPlayerDied(); 
    public static event OnPlayerDied onPlayerDied; 
    public delegate void OnSetCameraParameters(float cameraFOV, Vector3 centerScreen); 
    public static event OnSetCameraParameters onSetCameraParameters; 


    private void Start()
    {
        playerAnimation = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        _PlayerMouseLock = GetComponent<MouseLook>();
        _CameraMouseLock = MainCamera.GetComponent<MouseLook>();

        moveJump_r = transform.right;
        moveJump_f = transform.forward;

        InitCamera();
    }

    void Update()
    {
        #region Controls sounds animations
        if (!(Input.GetKey(Crawl_KeyControl) && Input.GetKey(Slink_KeyControl)) && Input.GetKeyDown(Jump_KeyControl) && isGrounded && stamina > 1f && !isLockedMovement)
        {
            velocity.y = Mathf.Sqrt(JumpForce * gravity);
            stamina -= SpeedSpendingStamina * 80;
            //anim.SetTrigger("Jump");
            if (UsingSounds)
            {
                Jumping_Source.clip = Jumping_Sounds[UnityEngine.Random.Range(0, Jumping_Sounds.Length)];
                Jumping_Source.Play();
            }
        }

        if (isRuning && (Input.GetAxis("Vertical") <= 0 || Input.GetAxis("Horizontal") != 0 || stamina < 15f)) isRuning = false;

        if (Input.GetKey(Run_KeyControl) && isLockedRunFlag)
        {
            if (isRuning) isRuning = false;
            else if (Input.GetAxis("Vertical") > 0 && stamina > 15f) isRuning = true;
            isLockedRunFlag = false;
            StartCoroutine(WairRestartFlagLockedRunKey());
        }

        if ((Input.GetKey(Crawl_KeyControl) && Input.GetKey(Slink_KeyControl) && !isLockedMovement) || (controller.height == HeightCrawl && isThereIsObjectAboveHead))
        {
            controller.height = HeightCrawl;
            speed = SpeedCrawl;
            isRuning = false;
        }
        else if (((Input.GetKey(Crawl_KeyControl) || Input.GetKey(Slink_KeyControl)) && !isLockedMovement) || (controller.height == HeightSlink && isThereIsObjectAboveHead))
        {
            controller.height = HeightSlink; 
            speed = SpeedSlink;
            isRuning = false;
        }
        else if (isRuning)
        {
            speed = SpeedRun;
            controller.height = HeightFull;
        }
        else
        {
            speed = SpeedWalk;
            controller.height = HeightFull;
            isRuning = false;
        }
        #endregion

        // анимация покачивания камеры
        if (isShakingCamera && Input.GetAxis("Vertical") != 0 && isGrounded && !isLockedMovement)
        {
            if (isRuning) HeadAnimation.SetInteger("st", 2);
            else HeadAnimation.SetInteger("st", 1);
        }
        else HeadAnimation.SetInteger("st", 0);

        // урон от падения
        if (velocity.y < minimumDropRateToTakeDamage) {
            isFellAtHighSpeed = true;
            isFellAtHighSpeed_lastVelocity = velocity.y;
        }
        if(isGrounded && isFellAtHighSpeed)
        {
            TakeDamage(dropDamageMultiplier * isFellAtHighSpeed_lastVelocity * -1);
            isFellAtHighSpeed_lastVelocity = 0;
            isFellAtHighSpeed = false;
        }

        #region Interaction
        if (Input.GetKey(Interaction_KeyControl))
        {           
            Ray ray = MainCamera.GetComponent<Camera>().ScreenPointToRay(screenCenter);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit))
            {
                if(hit.distance <= rangeOfInteraction && FlagDelayInteraction)
                {
                    onPlayerInteracts?.Invoke(hit, this); // отправить событие всем кто подписан и передать объект с которым взаимодействую и объект класса игрока

                    FlagDelayInteraction = false;
                    StartCoroutine(RestartInteraction());
                    
                }
            }

        }
        #endregion
    }

    private void OnTriggerEnter(Collider other)
    {      
         if (velocity.y > 0 && other.tag != "Player")
            TakeDamage(damageOfOctHeadbutt / 3);
        velocity.y = 0;

        if (other.tag != "Player")
            isThereIsObjectAboveHead = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player")
            isThereIsObjectAboveHead = false;       
    }

    private void FixedUpdate()
    {

        if (controller.isGrounded)
        {
            //anim.SetFloat("X", Input.GetAxis("Horizontal"));
            //anim.SetFloat("Y", Input.GetAxis("Vertical"));
        }
        #region Movment
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        if (isGrounded && !isLockedMovement)
        {
            movement.x = Input.GetAxis("Horizontal");
            movement.z = Input.GetAxis("Vertical");
            moveJump_r = transform.right;
            moveJump_f = transform.forward;
        }

        Vector3 move = moveJump_r * movement.x + moveJump_f * movement.z;
        if (!isLockedMovement) controller.Move(move * speed * Time.deltaTime);

        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        #endregion

        // звуки ходьбы
        if (UsingSounds && !isLockedMovement)
        {
            Walking_Source.pitch = map(speed, SpeedCrawl, SpeedRun, walking_SoundMinSpeed, walking_SoundMaxSpeed);
            if (!Walking_Source.isPlaying && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) && isGrounded)
            {
                Walking_Source.clip = Walking_Sound;              
                Walking_Source.Play();
            }

            if (Walking_Source.isPlaying && ((Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) || !isGrounded))
                Walking_Source.Stop();

        }

        // востановление здоровья и выносливости
        if (hunger > minimumHungerForRecoveryHealthAndStamina && thirst > minimumThirstForRecoveryHealthAndStamina)
        {
            if (health < 100 || stamina < 100)
            {
                health += SpeedRecoveryHealth;
                stamina += SpeedRecoveryStamina;
                // трата голода и жажды
                hunger -= SpeedSpendingHunger * 1.5f;
                thirst -= SpeedSpendingThirst * 1.5f;
            }
            if (health >= 100f) health = 100;
            if (stamina >= 100f) stamina = 100;
        }

        // трата голода и жажды
        hunger -= SpeedSpendingHunger;
        thirst -= SpeedSpendingThirst;

        // трата выносливости
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0 && !isLockedMovement)
        {
            if (isRuning) stamina -= SpeedSpendingStamina * SpeedRecoveryStamina_MultiplierRun;
            else if (speed == SpeedSlink) stamina -= SpeedSpendingStamina * SpeedRecoveryStamina_MultiplierSlink;           
            else if (speed == SpeedCrawl) stamina -= SpeedSpendingStamina * SpeedRecoveryStamina_MultiplierCrawl;           
            else if (speed == SpeedWalk) stamina -= SpeedSpendingStamina * SpeedRecoveryStamina_MultiplierWalk;           
        }
        if (stamina < 2f) IsLockedMovement = true;
        else if (stamina > 5f) IsLockedMovement = false;

    }

    public void TakeDamage(float damage, bool playSound = true)
    {
        health-= damage;
        if(health <= 0)
        {
            if (UsingSounds)
            {
                Damage_Source.clip = Death_Sound;
                Damage_Source.Play();
            }
            // смерть**************************************************
            onPlayerDied.Invoke();
        }
        if (playSound && UsingSounds)
        {
            Damage_Source.clip = TakeDamage_Sounds[UnityEngine.Random.Range(0, TakeDamage_Sounds.Length)];
            Damage_Source.Play();
        }
    }

    public void Eating(float value, bool playSound = true)
    {
        hunger += value;
        if (hunger > 100f) hunger = 100;
        if(playSound && UsingSounds)
        {
            Eating_Source.clip = Eating_Sounds[UnityEngine.Random.Range(0, Eating_Sounds.Length)];
            Eating_Source.Play();
        }
    }

    public void Drinking(float value, bool playSound = true)
    {
        thirst += value;
        if (thirst > 100f) thirst = 100;
        if(playSound && UsingSounds)
        {
            Drinking_Source.clip = Drinking_Sounds[UnityEngine.Random.Range(0, Drinking_Sounds.Length)];
            Drinking_Source.Play();
        }
    }

    public void Medication(float value, bool playSound = true)
    {
        health += value;
        if (health > 100f) health = 100;
        if(playSound && UsingSounds)
        {
            Medication_Source.clip = Medication_Sounds[UnityEngine.Random.Range(0, Medication_Sounds.Length)];
            Medication_Source.Play();
        }
    }
    
    public void StaminaAdding(float value)
    {
        stamina += value;
        if (stamina > 100f) stamina = 100;
    }

    public void InitCamera()
    {
        MainCamera.GetComponent<Camera>().fieldOfView = FOV_Camera;
        _CameraMouseLock.sensitivityX = MouseSensitivity;
        _CameraMouseLock.sensitivityY = MouseSensitivity;
        _CameraMouseLock.sensitivityX = MouseSensitivity;
        _CameraMouseLock.sensitivityY = MouseSensitivity;

        screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        onSetCameraParameters?.Invoke(FOV_Camera, screenCenter);
    }

    private IEnumerator WairRestartFlagLockedRunKey()
    {
        yield return new WaitForSeconds(.2f);
        isLockedRunFlag = true;
    }

    private IEnumerator RestartInteraction()
    {
        yield return new WaitForSeconds(delayReatartInteraction);
        FlagDelayInteraction = true;
    }

    public static float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}