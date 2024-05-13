using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using Mono.Data;
using System;
using System.Data;
using UnityEngine.UI;
using TMPro;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

public class AccountManager : MonoBehaviour
{
    public TextMeshProUGUI usernameField;
    public TMP_InputField passwordField;

    public GameObject usernameBox;
    public GameObject passwordBox;
    public GameObject invalidLoginText;
    public GameObject usernameTakenText;
    public GameObject loggedInText;
    public GameObject signedUpText;

    public string dataBaseName = "Accounts";
    public string signedInAccountName = "";

    public Camera camera;

    // for gliding movement
    public float glideSpeed = 83f;
    public float accelerationMultiplier = 10f;
    public float decelerationMultiplier = 20f;

    public DraftRunner[] scripts = new DraftRunner[8];

    public DraftRunner currentPlayer;

    // Start is called before the first frame update
    void Start()
    {
        resetNotifications();

        currentPlayer = GameObject.Find("Player Runner 1").GetComponent<DraftRunner>();
        
        for (int i = 0; i < scripts.Length; i++)
        {
            if (GameObject.Find("Player Runner " + (i + 1)) != null)
            {
                scripts[i] = GameObject.Find("Player Runner " + (i + 1)).GetComponent<DraftRunner>();
            }
            else
            {
                scripts[i] = GameObject.Find("Player Runner " + (i + 1) + " (bot)").GetComponent<DraftRunner>();
            }
        }
        usernameField = usernameBox.GetComponent<TextMeshProUGUI>();
        passwordField = passwordBox.GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool isMovingCamera = false;

    public void goToLogInScreen()
    {
        if (isMovingCamera)
        {
            return;
        }

        StartCoroutine(moveCameraToLogin());
    }

    public IEnumerator moveCameraToLogin()
    {
        Vector3 targetPosition = new Vector3(-61.4000015f, 19.9699993f, 10.3660002f);
        Vector3 initialPosition = camera.transform.position;

        isMovingCamera = true;

        float distance = Vector3.Distance(camera.transform.position, targetPosition);
        float currentSpeed = 0f;

        // Duration of acceleration and deceleration phases
        float accelerationDuration = 0.5f; // Adjust this for the desired acceleration duration
        float decelerationDuration = 1.0f; // Adjust this for the desired deceleration duration

        // Calculate acceleration and deceleration speeds
        float accelerationSpeed = glideSpeed / accelerationDuration;
        float decelerationSpeed = glideSpeed / decelerationDuration;

        // Start with acceleration phase
        float remainingAccelerationTime = accelerationDuration;
        
        Vector3 lastPos = camera.transform.position;
        while (distance > 0.01f)
        {
            // Update remaining time in acceleration phase
            remainingAccelerationTime -= Time.deltaTime;

            if (remainingAccelerationTime > 0f)
            {
                // Acceleration phase: speed up quickly
                currentSpeed = Mathf.MoveTowards(currentSpeed, glideSpeed, accelerationSpeed * Time.deltaTime);
            }
            else
            {
                // Deceleration phase: slow down gradually
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, decelerationSpeed * Time.deltaTime);
            }

            camera.transform.position = Vector3.MoveTowards(camera.transform.position, targetPosition, currentSpeed * Time.deltaTime);
            distance = Vector3.Distance(camera.transform.position, targetPosition);

            if (lastPos == camera.transform.position)
            {
                break;
            }
            else
            {
                lastPos = camera.transform.position;
            }

            yield return null;
        }   

        camera.transform.position = targetPosition;
        isMovingCamera = false;
    }
    

    public void goBackHome()
    {
        if (isMovingCamera)
        {
            return;
        }
        
        StartCoroutine(moveCamerBackHome());
    }

    public IEnumerator moveCamerBackHome()
    {
        Vector3 targetPosition = new Vector3(0f, 19.9699993f, 10.3660002f);
        Vector3 initialPosition = camera.transform.position;

        isMovingCamera = true;

        float distance = Vector3.Distance(camera.transform.position, targetPosition);
        float currentSpeed = 0f;

        // Duration of acceleration and deceleration phases
        float accelerationDuration = 0.5f; // Adjust this for the desired acceleration duration
        float decelerationDuration = 1.0f; // Adjust this for the desired deceleration duration

        // Calculate acceleration and deceleration speeds
        float accelerationSpeed = glideSpeed / accelerationDuration;
        float decelerationSpeed = glideSpeed / decelerationDuration;

        // Start with acceleration phase
        float remainingAccelerationTime = accelerationDuration;

        Vector3 lastPos = camera.transform.position;
        while (distance > 0.01f)
        {
            // Update remaining time in acceleration phase
            remainingAccelerationTime -= Time.deltaTime;

            if (remainingAccelerationTime > 0f)
            {
                // Acceleration phase: speed up quickly
                currentSpeed = Mathf.MoveTowards(currentSpeed, glideSpeed, accelerationSpeed * Time.deltaTime);
            }
            else
            {
                // Deceleration phase: slow down gradually
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, decelerationSpeed * Time.deltaTime);
            }

            camera.transform.position = Vector3.MoveTowards(camera.transform.position, targetPosition, currentSpeed * Time.deltaTime);
            distance = Vector3.Distance(camera.transform.position, targetPosition);

            if (lastPos == camera.transform.position)
            {
                break;
            }
            else
            {
                lastPos = camera.transform.position;
            }

            yield return null;
        }   

        camera.transform.position = targetPosition;
        isMovingCamera = false;

        resetNotifications();
    }

    
    public void startDraft(string formatChosen)
    {
        StartCoroutine(startDraftCamera(formatChosen));
    }


    public IEnumerator startDraftCamera(string formatChosen)
    {
        int speed = 27;
        Vector3 targetPosition = new Vector3(0f, 1f, 10.3660002f);
        Vector3 initialPosition = camera.transform.position;

        isMovingCamera = true;

        float distance = Vector3.Distance(camera.transform.position, targetPosition);
        float currentSpeed = 0f;

        // Duration of acceleration and deceleration phases
        float accelerationDuration = 0.5f; // Adjust this for the desired acceleration duration
        float decelerationDuration = 1.0f; // Adjust this for the desired deceleration duration

        // Calculate acceleration and deceleration speeds
        float accelerationSpeed = speed / accelerationDuration;
        float decelerationSpeed = speed / decelerationDuration;

        // Start with acceleration phase
        float remainingAccelerationTime = accelerationDuration;

        Vector3 lastPos = camera.transform.position;
        while (distance > 0.01f)
        {
            // Update remaining time in acceleration phase
            remainingAccelerationTime -= Time.deltaTime;

            if (remainingAccelerationTime > 0f)
            {
                // Acceleration phase: speed up quickly
                currentSpeed = Mathf.MoveTowards(currentSpeed, speed, accelerationSpeed * Time.deltaTime);
            }
            else
            {
                // Deceleration phase: slow down gradually
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, decelerationSpeed * Time.deltaTime);
            }

            camera.transform.position = Vector3.MoveTowards(camera.transform.position, targetPosition, currentSpeed * Time.deltaTime);
            distance = Vector3.Distance(camera.transform.position, targetPosition);

            if (lastPos == camera.transform.position)
            {
                break;
            }
            else
            {
                lastPos = camera.transform.position;
            }

            yield return null;
        }   

        camera.transform.position = targetPosition;
        isMovingCamera = false;

        for(int i = 0; i < scripts.Length; i++)
        {
            scripts[i].startDraft(formatChosen);
        }
    }

    public void signUp()
    {
        resetNotifications();

        if (isMovingCamera || usernameField.text.Trim() == "" || passwordField.text.Trim() == "")
        {
            return;
        }
        
        string dbFilePath = "Assets/_Databases/Accounts.db";

        string conn = "URI=file:" + dbFilePath;

        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                string username = usernameField.text.Trim();
                string password = passwordField.text.Trim();

                if (lookForAlreadyExistingAccount(username))
                {
                    usernameTakenText.SetActive(true);
                    passwordField.text = "";
                    Debug.Log("This Username Is Already Taken");
                    return;
                }

                string SQLQuery = "INSERT INTO " + dataBaseName + " (Username, Password, DeckList) VALUES (@Username, @Password, @DeckList)";
                dbCommand.CommandText = SQLQuery;

                IDbDataParameter paramUsername = dbCommand.CreateParameter();
                paramUsername.ParameterName = "@Username";
                paramUsername.Value = username;
                dbCommand.Parameters.Add(paramUsername);

                IDbDataParameter paramPassword = dbCommand.CreateParameter();
                paramPassword.ParameterName = "@Password";
                paramPassword.Value = password;
                dbCommand.Parameters.Add(paramPassword);

                IDbDataParameter paramDeckList = dbCommand.CreateParameter();
                paramDeckList.ParameterName = "@DeckList";
                paramDeckList.Value = "Empty";
                dbCommand.Parameters.Add(paramDeckList);

                dbCommand.ExecuteNonQuery();

                Debug.Log("Successfully Created An Account");
                
                signedUpText.SetActive(true);
            }
        }
    }

    public bool lookForAlreadyExistingAccount(string username)
    {
        string dbFilePath = "Assets/_Databases/Accounts.db";

        string conn = "URI=file:" + dbFilePath;

        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {

                string SQLQuery = "SELECT COUNT(*) FROM " + dataBaseName + " WHERE Username = @Username";
                dbCommand.CommandText = SQLQuery;

                IDbDataParameter paramUsername = dbCommand.CreateParameter();
                paramUsername.ParameterName = "@Username";
                paramUsername.Value = username;
                dbCommand.Parameters.Add(paramUsername);

                int count = Convert.ToInt32(dbCommand.ExecuteScalar());

                if (count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void checkLogin()
    {
        resetNotifications();
        if (isMovingCamera || usernameField.text.Trim() == "" || passwordField.text.Trim() == "")
        {
            return;
        }

        string dbFilePath = "Assets/_Databases/Accounts.db";

        string conn = "URI=file:" + dbFilePath;

        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            string username = usernameField.text.Trim();
            string password = passwordField.text.Trim();

            dbConnection.Open();
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                string SQLQuery = "SELECT COUNT(*) FROM " + dataBaseName + " WHERE Username = @Username AND Password = @Password";
                dbCommand.CommandText = SQLQuery;

                IDbDataParameter paramUsername = dbCommand.CreateParameter();
                paramUsername.ParameterName = "@Username";
                paramUsername.Value = username;
                dbCommand.Parameters.Add(paramUsername);

                IDbDataParameter paramPassword = dbCommand.CreateParameter();
                paramPassword.ParameterName = "@Password";
                paramPassword.Value = password;
                dbCommand.Parameters.Add(paramPassword);

                int count = Convert.ToInt32(dbCommand.ExecuteScalar());

                if (count > 0)
                {
                    signedInAccountName = username;
                    loggedInText.SetActive(true);
                    Debug.Log("Login Successful."); // Username and password match
                }
                else
                {
                    invalidLoginText.SetActive(true);
                    passwordField.text = "";
                    Debug.Log("Invalid Username Or Password."); // Username or password doesn't match
                }
            }
        }
    }

    public void updateDeckList()
    {
        if (signedInAccountName == "")
        {
            Debug.Log("You Must Sign In To Save Your Decklist");
            return;
        }

        // Specify the filepath of your SQLite database
        string dbFilePath = "Assets/_Databases/Accounts.db";

        // Construct the connection string
        string conn = "URI=file:" + dbFilePath;

        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                // Retrieve current DeckList from the database
                string SQLQuerySelect = "SELECT DeckList FROM " + dataBaseName + " WHERE Username = @Username";
                dbCommand.CommandText = SQLQuerySelect;

                IDbDataParameter paramUsernameSelect = dbCommand.CreateParameter();
                paramUsernameSelect.ParameterName = "@Username";
                paramUsernameSelect.Value = signedInAccountName;
                dbCommand.Parameters.Add(paramUsernameSelect);

                using (IDataReader reader = dbCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string currentDeckList = reader.GetString(0);

                        if (currentDeckList.IndexOf("Empty") != -1)
                        {
                            currentDeckList = currentDeckList.Substring(currentDeckList.IndexOf("Empty") + 5);
                        }

                        // Close the reader before modifying the command text
                        reader.Close();

                        // Append newDeckList to currentDeckList
                        string listToAdd = "[" + currentPlayer.deckList[0];
                        for (int i = 1; i < currentPlayer.deckList.Count; i++)
                        {
                            listToAdd += ", " + currentPlayer.deckList[i];
                        }
                        listToAdd += "][";
                        for (int i = 1; i < currentPlayer.sideboardList.Count; i++)
                        {
                            listToAdd += ", " + currentPlayer.sideboardList[i];
                        }
                        listToAdd += "] ";

                        string updatedDeckList = currentDeckList + listToAdd;

                        // Update the database with the updated DeckList
                        string SQLQueryUpdate = "UPDATE " + dataBaseName + " SET DeckList = @DeckList WHERE Username = @Username";
                        dbCommand.CommandText = SQLQueryUpdate;

                        IDbDataParameter paramDeckListUpdate = dbCommand.CreateParameter();
                        paramDeckListUpdate.ParameterName = "@DeckList";
                        paramDeckListUpdate.Value = updatedDeckList;
                        dbCommand.Parameters.Add(paramDeckListUpdate);

                        // Add Username parameter (if not already added)
                        if (!dbCommand.Parameters.Contains(paramUsernameSelect))
                        {
                            dbCommand.Parameters.Add(paramUsernameSelect);
                        }

                        // ExecuteNonQuery for UPDATE operation
                        dbCommand.ExecuteNonQuery();

                        Debug.Log("Successfully Saved Your Decklist");
                    }
                }
            }
        }
    }

    public void resetNotifications()
    {
        invalidLoginText.SetActive(false);
        usernameTakenText.SetActive(false);
        loggedInText.SetActive(false);
        signedUpText.SetActive(false);
    }
}