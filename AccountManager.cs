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

    public string dataBaseName = "Accounts";
    public string signedInAccountName = "";

    public Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        usernameField = usernameBox.GetComponent<TextMeshProUGUI>();
        passwordField = passwordBox.GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void goToLogInScreen()
    {
        camera.transform.position = new Vector3(-125.400002f, 1f, 10.3660002f);
    }

    public void goBackHome()
    {
        camera.transform.position = new Vector3(-50f, 1f, 10.3660002f);
    }

    public void signUp()
    {
        string dbFilePath = "Assets/_Databases/Accounts.db";

        string conn = "URI=file:" + dbFilePath;

        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                string username = usernameField.text;
                string password = passwordField.text;

                if (lookForAlreadyExistingAccount(username))
                {
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
                    Debug.Log("Username already exists.");
                    return true;
                }
                else
                {
                    Debug.Log("Username does not exist.");
                    return false;
                }
            }
        }

        return true;
    }

    public void checkLogin()
    {
        string dbFilePath = "Assets/_Databases/Accounts.db";

        string conn = "URI=file:" + dbFilePath;

        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            string username = usernameField.text;
            string password = passwordField.text;

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
                    Debug.Log("Login successful."); // Username and password match
                }
                else
                {
                    Debug.Log("Invalid username or password."); // Username or password doesn't match
                }
            }
        }
    }

    public void updateDeckList(string username, string newDeckList)
    {
        // Specify the filepath of your SQLite database
        string dbFilePath = "Assets/_Databases/Accounts.db";

        // Construct the connection string
        string conn = "URI=file:" + dbFilePath;

        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                // Use parameterized query to prevent SQL injection
                string SQLQuery = "UPDATE " + dataBaseName + " SET DeckList = @DeckList WHERE Username = @Username";
                dbCommand.CommandText = SQLQuery;

                // Add parameters
                IDbDataParameter paramUsername = dbCommand.CreateParameter();
                paramUsername.ParameterName = "@Username";
                paramUsername.Value = username;
                dbCommand.Parameters.Add(paramUsername);

                IDbDataParameter paramDeckList = dbCommand.CreateParameter();
                paramDeckList.ParameterName = "@DeckList";
                paramDeckList.Value = newDeckList;
                dbCommand.Parameters.Add(paramDeckList);

                // ExecuteNonQuery for UPDATE operation
                dbCommand.ExecuteNonQuery();
            }
        }
    }
}
