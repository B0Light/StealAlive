using TMPro;
using UnityEngine;

public class UI_CharacterHPBar : UI_StatBarSlider
{
    private CharacterManager character;
    private AICharacterManager aiCharacter;

    [SerializeField] bool displayCharacterNameOnDamage = false;
    [SerializeField] float defaultTimeBeforeBarHides = 3;
    [SerializeField] float hideTimer = 0;
    [SerializeField] int currentDamageTaken = 0;
    [SerializeField] TextMeshProUGUI characterName;
    [SerializeField] TextMeshProUGUI characterDamage;
    [HideInInspector] public int oldHealthValue = 0;

    protected override void Awake()
    {
        base.Awake();

        character = GetComponentInParent<CharacterManager>();

        if (character != null)
        {
            aiCharacter = character as AICharacterManager;
        }
    }

    private void Start()
    {
        gameObject.SetActive(false);
        oldHealthValue = character.characterVariableManager.health.Value;
    }

    private void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.forward);

        if (hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        currentDamageTaken = 0;
        oldHealthValue = character.characterVariableManager.health.Value;
    }

    public override void SetStat(int newValue)
    {
        if (displayCharacterNameOnDamage)
        {
            characterName.enabled = true;

            if (aiCharacter != null)
            {
                Debug.Log("CHARACTER NAME : " + aiCharacter.characterName);
                characterName.text = aiCharacter.characterName;
            }
            else
            {
                Debug.Log("NO AI CHARACTER");
            }
        }

        //  CALL THIS HERE INCASE MAX HEALTH CHANGES FROM A CHARACTER EFFECT/BUFF ETC...
        slider.maxValue = character.characterVariableManager.health.MaxValue;

        //  TO DO: RUN SECONDARY BAR LOGIC (YELLOW BAR THAT APPEARS BEHIND HP WHEN DAMAGED)

        //  TOTAL THE DAMAGE TAKEN WHILST THE BAR IS ACTIVE
        currentDamageTaken = Mathf.RoundToInt(oldHealthValue - newValue);

        if (currentDamageTaken < 0)
        {
            currentDamageTaken = Mathf.Abs(currentDamageTaken);
            characterDamage.text = "+ " + currentDamageTaken.ToString();
        }
        else
        {
            characterDamage.text = "- " + currentDamageTaken.ToString();
        }

        slider.value = newValue;

        if (character.characterVariableManager.health.Value != character.characterVariableManager.health.MaxValue)
        {
            hideTimer = defaultTimeBeforeBarHides;
            gameObject.SetActive(true);
        }
    }
}
