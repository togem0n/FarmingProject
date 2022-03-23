using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : SingletonMonoBehaviour<Player>
{
    [SerializeField] public PlayerData playerData;
    [SerializeField] public GridCursor gridCursor;
        
    #region StateMachine
    public PlayerStateMachine Statemachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerWalkState WalkState { get; private set; }
    public PlayerHoeingState HoeingState { get; private set; }
    public PlayerWateringState WateringState { get; private set; }
    public PlayerCarrySeedState CarryingSeedState { get; private set; }
    public PlayerHarvestingState HarvestingState { get; private set; }
    public PlayerChoppingState ChoppingState { get; private set; }
    public PlayerBreakingState BreakingState { get; private set; }

    #endregion

    #region Movement
    public int xInput;
    public int yInput;
    public Vector2 inputVector;
    public Vector2 moveDirection;
    public float movementSpeed;
    public Direction playerDirection;
    public bool _playerInputDisabled = false;

    public bool PlayerInputDisabled { get => _playerInputDisabled; set => _playerInputDisabled = value; }
    #endregion

    #region Components
    public Rigidbody2D rb;
    public Animator animator;
    public Camera mainCamera;
    #endregion

    #region Use Tool Variables
    public Vector3 useToolDirection;
    public float useToolDirectionForAnimator;
    public Vector3Int useToolGridDirection;
    public Vector3Int useToolGridPosition;
    #endregion

    #region Others
    public Vector2 CurrentVelocity { get; private set; }
    public int FacingDirection { get; private set; }
    public Vector2 workspace;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        Statemachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, Statemachine, playerData, "idle");
        WalkState = new PlayerWalkState(this, Statemachine, playerData, "walk");
        HoeingState = new PlayerHoeingState(this, Statemachine, playerData, "hoeing");
        WateringState = new PlayerWateringState(this, Statemachine, playerData, "watering");
        CarryingSeedState = new PlayerCarrySeedState(this, Statemachine, playerData, "planting");
        HarvestingState = new PlayerHarvestingState(this, Statemachine, playerData, "harvesting");
        ChoppingState = new PlayerChoppingState(this, Statemachine, playerData, "chopping");
        BreakingState = new PlayerBreakingState(this, Statemachine, playerData, "breaking");
    }

    private void Start()
    {
        EventHandler.AfterSceneLoadFadeInEvent += EnablePlayerInput;

        FacingDirection = 1;
        Statemachine.Initialize(IdleState);
    }

    private void Update()
    {

        if (!PlayerInputDisabled)
        {
            GetPlayerInput();

            PlayerTestInput();
        }

        Statemachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        Statemachine.CurrentState.PhysicsUpdate();
    }

    private void GetPlayerInput()
    {
        if (!_playerInputDisabled)
        {
            xInput = (int)Input.GetAxisRaw("Horizontal");
            yInput = (int)Input.GetAxisRaw("Vertical");
            inputVector = new Vector2(xInput, yInput);

            if (!Mathf.Approximately(inputVector.x, 0.0f) || !Mathf.Approximately(inputVector.y, 0.0f))
            {
                moveDirection.Set(inputVector.x, inputVector.y);
                moveDirection.Normalize();
            }

            if(xInput != 0 || yInput != 0)
            {
                movementSpeed = playerData.movementSpeed;

                if (xInput < 0)
                {
                    playerDirection = Direction.left;
                }
                else if (xInput > 0)
                {
                    playerDirection = Direction.right;
                }
                else if (yInput < 0)
                {
                    playerDirection = Direction.down;
                }
                else if (yInput > 0)
                {
                    playerDirection = Direction.up;
                }
            }
        }
    }

    public Vector3 GetPlyerViewportPosition()
    {
        return mainCamera.WorldToViewportPoint(transform.position);
    }

    public void EnablePlayerInput()
    {
        PlayerInputDisabled = false;
    }

    public void DisablePlayerInput()
    {
        PlayerInputDisabled = true;
    }

    public void DisablePlayerInputAndResetMovement()
    {
        DisablePlayerInput();

        ResetMovement();
    }

    private void ResetMovement()
    {
        xInput = 0;
        yInput = 0;
        animator.SetFloat("xInput", moveDirection.x);
        animator.SetFloat("yInput", moveDirection.y);
        animator.SetFloat("speed", 0);
    }

    private void PlayerTestInput()
    {
        if (Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }

        if (Input.GetKeyUp(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameDay();
        }

        if (Input.GetKey(KeyCode.L))
        {
            SceneControllerManager.Instance.FadeAndLoadScene(SceneName.Scene1_Farm.ToString(), transform.position);
        }
    }

    public void SetUseToolDirection(float mousePosX, float mousePosY)
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosX, mousePosY, -mainCamera.transform.position.z));
        Vector3 tmp = mouseWorldPosition - transform.position;

        Vector3Int GridOfMouse = GridPropertyManager.Instance.grid.WorldToCell(mouseWorldPosition);
        Vector3Int GridOfPlayer = GridPropertyManager.Instance.grid.WorldToCell(transform.position);

        int itemUseGridRadius = InventoryManager.Instance.GetSelectedItemDetails().itemUseGridRadius;

        if(Mathf.Abs(GridOfMouse.x - GridOfPlayer.x) <= itemUseGridRadius && Mathf.Abs(GridOfMouse.y - GridOfPlayer.y) <= itemUseGridRadius)
        {

            useToolGridPosition = GridOfMouse;

            useToolGridDirection = GridOfMouse - GridOfPlayer;

            if (GridOfMouse == GridOfPlayer)
            {
                useToolGridDirection.x = (int)moveDirection.x;
                useToolGridDirection.y = (int)moveDirection.y;

                useToolGridPosition = GridOfPlayer + useToolGridDirection;
            }
            

            if(useToolGridDirection.y != 0)
            {
                useToolGridDirection.x = 0;
            }
            moveDirection.x = useToolGridDirection.x;
            moveDirection.y = useToolGridDirection.y;

        }
        else
        {
            useToolGridDirection.x = (int)moveDirection.x;
            useToolGridDirection.y = (int)moveDirection.y;

            useToolGridPosition = GridOfPlayer + useToolGridDirection;
        }
        
    }

    public void SetPlantDirection(float mousePosX, float mousePosY)
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosX, mousePosY, -mainCamera.transform.position.z));
        Vector3 tmp = mouseWorldPosition - transform.position;

        Vector3Int GridOfMouse = GridPropertyManager.Instance.grid.WorldToCell(mouseWorldPosition);
        Vector3Int GridOfPlayer = GridPropertyManager.Instance.grid.WorldToCell(transform.position);

        int itemUseGridRadius = InventoryManager.Instance.GetSelectedItemDetails().itemUseGridRadius;

        if (Mathf.Abs(GridOfMouse.x - GridOfPlayer.x) <= itemUseGridRadius && Mathf.Abs(GridOfMouse.y - GridOfPlayer.y) <= itemUseGridRadius)
        {

            useToolGridPosition = GridOfMouse;

            useToolGridDirection = GridOfMouse - GridOfPlayer;

            if (GridOfMouse == GridOfPlayer)
            {
                useToolGridDirection.x = (int)moveDirection.x;
                useToolGridDirection.y = (int)moveDirection.y;

                useToolGridPosition = GridOfPlayer;
                return;
            }


            if (useToolGridDirection.y != 0)
            {
                useToolGridDirection.x = 0;
            }
            moveDirection.x = useToolGridDirection.x;
            moveDirection.y = useToolGridDirection.y;

        }
        else
        {
            useToolGridDirection.x = (int)moveDirection.x;
            useToolGridDirection.y = (int)moveDirection.y;

            useToolGridPosition = GridOfPlayer + useToolGridDirection;
        }
    }

    private void AnimationTrigger() => Statemachine.CurrentState.AnimationTrigger();

    private void AniamtionFinishTrigger() => Statemachine.CurrentState.AnimationFinishTrigger();

}
