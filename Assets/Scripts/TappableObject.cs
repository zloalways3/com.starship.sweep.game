using UnityEngine;

public class TappableObject : MonoBehaviour
{
    private GameController gameController;
    public Sprite[] planetSprites;

    void Start()
    {
        gameController = FindObjectOfType<GameController>();

        if (this.gameObject.tag == "Planet")
        {
            GetComponent<SpriteRenderer>().sprite = planetSprites[Random.Range(0, planetSprites.Length)];
        }
    }

    void OnMouseDown()
    {
        if (gameObject.tag == "Planet")
        {
            FindObjectOfType<AudioManager>().PlaySoundByIndex(3);
        }
        else { FindObjectOfType<AudioManager>().PlaySoundByIndex(0); }
        gameController.ObjectTapped(gameObject);
    }
}
