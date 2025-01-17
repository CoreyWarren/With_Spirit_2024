using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    //If you're looking for WTF "Dialogue" is,
    //or, for example, where the "sentences" and "number of sentences"
    //declarations are, check out the "Dialogue" script,
    //it is not referenced directly by the objects, so it may be hard
    //to find. Took me a minute to find it, lol.

    [HideInInspector]
    public static bool dialogueOn;
    private Queue<string> sentences;
    GameObject dialogueBox, dialogueText;
    AudioSource as1;
    private AudioClip talk1;
    int charNumber;
    float defaultPitch;
    //public GameObject dialogueImageObj;
    Animator dialogueAnimator;
    //public RuntimeAnimatorController dialogueImageAnimator;
    //public Image dialogueImage;
    private int sentenceCounter;
    public Dialogue dialogueTransient;
    // Set a typing speed delay (adjustable)
    [SerializeField]
    private float typingDelay = 0.4f; // Adjust this to make typing faster or slower

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
        dialogueBox = GameObject.FindWithTag("Dialogue Box");
        dialogueText = GameObject.FindWithTag("Dialogue Text");
        dialogueOn = false;
        as1 = GetComponent<AudioSource>();
        charNumber = 0;
        defaultPitch = as1.pitch;
        //dialogueImageObj = GameObject.FindGameObjectWithTag("Dialogue Image");
        dialogueAnimator = GameObject.FindGameObjectWithTag("Dialogue Image").GetComponent<Animator>();
        sentenceCounter = 0;

    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialogueOn = true;

        Debug.Log("Starting convo with" + dialogue.speaker);
        talk1 = dialogue.myVoice;
        //Change the sole Dialogue Image to the correct face for this sentence
        //dialogueImage.sprite = dialogue.myFace[sentenceCounter];

        sentences.Clear();

        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        dialogueTransient = dialogue;
        
        
        DisplayNextSentence();

        
    }

    public void DisplayNextSentence()
    {
        
        if (sentences.Count == 0)
        {
            EndDialogue();
            sentenceCounter = 0;
            return;
        }

        dialogueAnimator.runtimeAnimatorController = dialogueTransient.myAnimator[sentenceCounter];
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
        Debug.Log(sentence);
        
        sentenceCounter++;
    }


    IEnumerator TypeSentence(string sentence)
    {
        // Clear the dialogue text before typing begins
        dialogueText.GetComponent<TextMeshProUGUI>().text = "";



        foreach (char letter in sentence.ToCharArray())
        {
            // Add the character to the dialogue text
            dialogueText.GetComponent<TextMeshProUGUI>().text += letter;
            charNumber++;

            // Play a sound every 4 characters
            if (charNumber % 4 == 0)
            {
                as1.Stop();
                as1.PlayOneShot(talk1);
                as1.pitch = defaultPitch + Random.Range(0f, 0.15f);
            }

            // Wait using unscaled time
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < startTime + typingDelay)
            {
                yield return null; // Wait until the delay has passed
            }
        }
    }


    void EndDialogue()
    {
        Debug.Log("End of conversation.");
        pauser1.endDialogue = true;
        dialogueOn = false;
    }



}
