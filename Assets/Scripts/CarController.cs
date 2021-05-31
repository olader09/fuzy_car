using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
	Rigidbody2D rb;
	BoxCollider2D col;

	//Ускорение
	[SerializeField]
	float accelerationPower = 2f;

	//КрУтОсТь ПоВоРоТа РуЛя
	[SerializeField]
	float steeringPower = 0.3f;

	//Максимальная скорость
	[SerializeField]
	float maxSpeed = 8f;

	//Показатели датчиков
	[SerializeField]
	float LeftSensorNum = 0f;
	[SerializeField]
	float LeftCenterSensorNum = 0f;
	[SerializeField]
	float CenterSensorNum = 0f;
	[SerializeField]
	float RightCenterSensorNum = 0f;
	[SerializeField]
	float RightSensorNum = 0f;

	//Был ли удар
	[SerializeField]
	bool IsHit = false;

	//Максимальная дальность сенсора
	[SerializeField]
	float MaxSensorNum = 5f;

	//Сенсоры
	[SerializeField]
	Transform LeftSensor;
	[SerializeField]
	Transform LeftCenterSensor;
	[SerializeField]
	Transform RightCenterSensor;
	[SerializeField]
	Transform RightSensor;

	//Рендеры линий
	[SerializeField]
	LineRenderer LeftRenderer;
	[SerializeField]
	LineRenderer LeftCenterRenderer;
	[SerializeField]
	LineRenderer RightCenterRenderer;
	[SerializeField]
	LineRenderer RightRenderer;

    //Для полей ввода
    [SerializeField]
    InputField InputSpeed;
    [SerializeField]
    InputField InputLength;

    Vector2 startPos;

    bool pause;

    float steeringAmount, speed, acc, direction, gas, startRot;
    
    //Для изменения значения скорости
    public void InSpeed(string Atext)
    {
        float res;
        bool flag = float.TryParse(Atext, out res);
        if (flag && res <= 10 && res > 0)
            maxSpeed = res;
        else if (!flag && Atext == "") { }
        else
        {
            maxSpeed = 8;
            InputSpeed.text = maxSpeed.ToString();
        }
    }
    //Для изменения значения длинны сканеров
    public void InLength(string Atext)
    {
        float res;
        bool flag = float.TryParse(Atext, out res);
        if (flag && res <= 7 && res > 0)
            MaxSensorNum = res;
        else if (!flag && Atext == "") { }
        else
        {
            MaxSensorNum = 5;
            InputLength.text = MaxSensorNum.ToString();
        }
    }
    //Для установки в начале программы стартового значения длинны сканеров
    public void SetInputLength()
    {
        InputLength.text = MaxSensorNum.ToString();
    }
    //Для установки в начале программы стартового значения скорости
    public void SetInputSpeed()
    {
        InputSpeed.text = maxSpeed.ToString();
    }

	public void LRotate()
	{
		if (pause == true){
			rb = GetComponent<Rigidbody2D>();
			rb.rotation += 10;
		}
		else {
			return ;
		}
    }

	public void RRotate()
	{
		if (pause == true){
			rb = GetComponent<Rigidbody2D>();
			rb.rotation -= 10;
		}
		else {
			return ;
		}
    }

    // Use this for initialization
    void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		col = GetComponent<BoxCollider2D>();
		//Не трогать!!! МАГИЯ
		LeftRenderer.sortingOrder = 4; 
		LeftRenderer.sortingLayerName = "UI";
        SetInputSpeed();
        SetInputLength();
        startPos = rb.position;
        startRot = rb.rotation;
        pause = true;
        speed = 0;
    }

    private float[] AI(float l, float lc, float rc, float r)
    {
        float[] res = new float[2];
        res[0] = 0;
        res[1] = 1;
        float rk = (1 -l / MaxSensorNum) + (1- lc / MaxSensorNum); //коэф поворота направо
        float lk = (1 - r / MaxSensorNum) + (1 - rc / MaxSensorNum); //коэф поворота налево
        float ck = Math.Min(lc / MaxSensorNum, rc / MaxSensorNum) *(1 - speed/maxSpeed); // коэф разгона
        if (ck < (float)0.1)
            ck = 0;

        if (l < MaxSensorNum || r < MaxSensorNum || lc < MaxSensorNum || rc < MaxSensorNum)
        {
            res[0] += rk > lk ? rk: -lk; // какой кэф больше , туда и поворачиваем
            res[1] *= ck ;
            UnityEngine.Debug.Log(res[1]);
        }

        return res;
    }

    public void gamePause() => pause = true;
    public void gameUnpause() => pause = false;
    public void gameRestart()
    {
        gamePause();
        rb.position = startPos;
        rb.rotation = startRot;
    }


    // Update is called once per frame
    void FixedUpdate()
	{
        if (pause) return;
        //Входные параметры Input (от -1 до 1) их заменить на данные с экспертной системы (а пока можно стрелками управлять)
        steeringAmount = -1 * AI(LeftSensorNum, LeftCenterSensorNum, RightCenterSensorNum , RightSensorNum)[0];
        acc = AI(LeftSensorNum, LeftCenterSensorNum, RightCenterSensorNum, RightSensorNum)[1];
		//Автоматически жмём на тормоз
		if (acc == 0 && speed != 0)
			acc = -1;

		//Поворачиваем машину
		direction = Mathf.Sign(Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.up)));
		rb.rotation += steeringAmount * steeringPower * speed * direction;

		//Увеличиваем скорость
		if (maxSpeed >= speed)
			speed = speed + acc * accelerationPower * 5 * Time.deltaTime;
		else
			speed = maxSpeed;

		//Отсекаем задний ход
		if (speed < 0)
			speed = 0;

		Vector2 v = transform.up;

		//Передвигаем машину
		rb.position += speed * v * Time.deltaTime;

		//Левый сенсор
		RaycastHit2D hitLeft = Physics2D.Raycast(LeftSensor.position, LeftSensor.up, MaxSensorNum);
		if (hitLeft.collider != null)
		{
			if (hitLeft.collider.tag != "finish")
			{
				LeftSensorNum = hitLeft.distance;
				LeftRenderer.enabled = true;
				LeftRenderer.SetVertexCount(2);
				LeftRenderer.SetPosition(0, LeftSensor.position);
				LeftRenderer.SetPosition(1, hitLeft.point);

			}
			else
			{
				LeftRenderer.enabled = false;
				LeftSensorNum = MaxSensorNum;
			}
		}
		else
		{
			LeftRenderer.enabled = false;
			LeftSensorNum = MaxSensorNum;
		}

		//Левый сенсор спереди
		RaycastHit2D hitLeftCenter = Physics2D.Raycast(LeftCenterSensor.position, LeftCenterSensor.up, MaxSensorNum);
		if (hitLeftCenter.collider != null)
		{
			if (hitLeftCenter.collider.tag != "finish")
			{
				LeftCenterSensorNum = hitLeftCenter.distance;
				LeftCenterRenderer.enabled = true;
				LeftCenterRenderer.SetVertexCount(2);
				LeftCenterRenderer.SetPosition(0, LeftCenterSensor.position);
				LeftCenterRenderer.SetPosition(1, hitLeftCenter.point);
			}
			else
			{
				LeftCenterRenderer.enabled = false;
				LeftCenterSensorNum = MaxSensorNum;
			}
		}
		else
		{
			LeftCenterRenderer.enabled = false;
			LeftCenterSensorNum = MaxSensorNum;
		}

		//Правый сенсор спереди
		RaycastHit2D hitRightCenter = Physics2D.Raycast(RightCenterSensor.position, RightCenterSensor.up, MaxSensorNum);
		if (hitRightCenter.collider != null)
		{
					if (hitRightCenter.collider.tag != "finish")
					{
						RightCenterSensorNum = hitRightCenter.distance;
						RightCenterRenderer.enabled = true;
						RightCenterRenderer.SetVertexCount(2);
						RightCenterRenderer.SetPosition(0, RightCenterSensor.position);
						RightCenterRenderer.SetPosition(1, hitRightCenter.point);
					}
			else
			{
				RightCenterRenderer.enabled = false;
				RightCenterSensorNum = MaxSensorNum;
			}

		}
		else
		{
			RightCenterRenderer.enabled = false;
			RightCenterSensorNum = MaxSensorNum;
		}


		//Правый сенсор
		RaycastHit2D hitRight = Physics2D.Raycast(RightSensor.position, RightSensor.up, MaxSensorNum);
		if (hitRight.collider != null)
		{
			if (hitRight.collider.tag != "finish")
			{
				RightSensorNum = hitRight.distance;
				RightRenderer.enabled = true;
				RightRenderer.SetVertexCount(2);
				RightRenderer.SetPosition(0, RightSensor.position);
				RightRenderer.SetPosition(1, hitRight.point);
			}
			else
			{
				RightRenderer.enabled = false;
				RightSensorNum = MaxSensorNum;
			}
		}
		else
		{
			RightRenderer.enabled = false;
			RightSensorNum = MaxSensorNum;
		}
        InSpeed(InputSpeed.text);
        InLength(InputLength.text);
    }

    private void OnColliderEnter2D(Collider2D collision) {
        if (collision.tag == "finish")
        {
            gameRestart();
        }
    }

    //Произошло столкновение
    void OnCollisionEnter2D(Collision2D collision)
	{
        if (collision.collider.tag == "finish")
        {
            gameRestart();
        }
        else
        {
            UnityEngine.Debug.Log("Collision!");
            IsHit = true;
        }
	}
}
