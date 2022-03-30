using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UserData", menuName = "ScriptableObjects/userData", order = 1)]
public class UserData : ScriptableObject
{
    public int userId; // ����� ��ȣ
    public string email; // ����� �̸���
    public string userName; // ����� �̸�
    public string password; // ��й�ȣ
}
