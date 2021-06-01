using System;
using UnityEngine;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
	Rigidbody2D RigidBody;
	BoxCollider2D col;

	[SerializeField]
	float accelerationPower = 2f;

	[SerializeField]
	float steeringPower = 0.3f;

	//Максимальная скорость
	[SerializeField]
	float maxSpeed = 8f;

	//данные сенсоров
	[SerializeField]
	float LeftSensorNum = 0f;
	[SerializeField]
	float LeftCenterSensorNum = 0f;
	[SerializeField]
	float RightCenterSensorNum = 0f;
	[SerializeField]
	float RightSensorNum = 0f;

	//Максимальная дальность сенсора
	[SerializeField]
	float MaxSensorNum = 5f;

	//Сенсоры
	[SerializeField]
	Transform FinishSensor;
	[SerializeField]
	Transform LeftSensor;
	[SerializeField]
	Transform LeftCenterSensor;
	[SerializeField]
	Transform RightCenterSensor;
	[SerializeField]
	Transform RightSensor;

	//Прорисовка сенсоров
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
	Vector2 finishPos;

	bool pause;

	float steeringAmount, speed, acc, direction, gas, startRot;

	public bool fromTheLeft(Vector2 vect)
	{
		double angle;
		if (vect.x > 0)
		{
			angle = Math.Atan2(vect.y, vect.x);
		}
		else
		{
			angle = Math.Atan2(vect.y, vect.x);
		}
		angle = angle * 180 / 3.14;
		var target = (Math.Round(RigidBody.rotation) + 90) % 360;

		/*if (angle < -90)
        {
			angle += 180;
        }
		if (angle > 90)
        {
			angle -= 180;
        }

		if (target < -180)
        {
			target += 360;
        }
		if (target > 180)
        {
			target -= 360;
        }*/

		if (target < 0)
		{
			target = 360 - target;
		}
		//Debug.Log(target);
		//Debug.Log(angle);
		if (Math.Abs(angle - target) < 180)
		{
			return (angle - target) > 0;
		}
		else
		{
			return (angle - target) < 0;
		}
		//return angle - target > 0;
	}

	public bool fromTheRight(Vector2 vect)
	{
		double angle;
		if (vect.x > 0)
		{
			angle = Math.Atan2(vect.y, vect.x);
		}
		else
		{
			angle = Math.Atan2(vect.y, vect.x);
		}
		angle = angle * 180 / 3.14;
		var target = (Math.Round(RigidBody.rotation) + 90) % 360;

		/*if (angle < -90)
        {
			angle += 180;
        }
		if (angle > 90)
        {
			angle -= 180;
        }

		if (target < -180)
        {
			target += 360;
        }
		if (target > 180)
        {
			target -= 360;
        }*/


		if (target < 0)
		{
			target = 360 - target;
		}
		if (Math.Abs(angle - target) < 180)
		{
			return (angle - target) < 0;
		}
		else
		{
			return (angle - target) > 0;
		}

		//return angle - target < 0;
	}

	//Для изменения значения скорости
	public void InSpeed(string Atext)
	{
		float res;
		bool success = float.TryParse(Atext, out res);
		if (success && res <= 10 && res > 0)
			maxSpeed = res;
		else if (!success && Atext == "") { }
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
		bool success = float.TryParse(Atext, out res);
		if (success && res <= 7 && res > 0)
			MaxSensorNum = res;
		else if (!success && Atext == "") { }
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
			RigidBody = GetComponent<Rigidbody2D>();
			RigidBody.rotation += 10;
		}
		else {
			return ;
		}
    }

	public void RRotate()
	{
		if (pause == true){
			RigidBody = GetComponent<Rigidbody2D>();
			RigidBody.rotation -= 10;
		}
		else {
			return ;
		}
    }

	void Start()
	{
		RigidBody = GetComponent<Rigidbody2D>();
		col = GetComponent<BoxCollider2D>();
		LeftRenderer.sortingOrder = 4;
		LeftRenderer.sortingLayerName = "UI";
		SetInputSpeed();
		SetInputLength();
		startPos = RigidBody.position;
		startRot = RigidBody.rotation;
		var finish = GameObject.FindGameObjectsWithTag("finish");
		finishPos = finish[0].transform.position;
		Debug.Log(finishPos);
		pause = true;
		speed = 0;
	}

	private float[] AI(float l, float lc, float rc, float r)
	{
		float[] res = new float[2];
		res[0] = 0;
		res[1] = 1;
		float rightCoeff = (1 - l / MaxSensorNum) + (1 - lc / MaxSensorNum);
		float leftCoeff = (1 - r / MaxSensorNum) + (1 - rc / MaxSensorNum);
		float speedCoeff = Math.Min(lc / MaxSensorNum, rc / MaxSensorNum) * (1 - speed / maxSpeed);
		if (speedCoeff < (float)0.1)
			speedCoeff = 0;

		if (l < MaxSensorNum || r < MaxSensorNum || lc < MaxSensorNum || rc < MaxSensorNum)
		{
			res[0] += rightCoeff > leftCoeff ? rightCoeff : -leftCoeff;
			res[1] *= speedCoeff;
		}

		else if (fromTheLeft(finishPos))
		{
			leftCoeff += 0.5f;
			res[0] += -leftCoeff;
			Debug.Log("Sleva");
		}

		else if (fromTheRight(finishPos))
		{
			rightCoeff += 2f;
			res[0] += rightCoeff;
			Debug.Log("Sprava");
		}
		return res;
	}

	public void gamePause() => pause = true;
	public void gameUnpause() => pause = false;
	public void gameRestart()
	{
		gamePause();
		RigidBody.position = startPos;
		RigidBody.rotation = startRot;
	}

	void FixedUpdate()
	{
		if (pause) return;

		steeringAmount = -1 * AI(LeftSensorNum, LeftCenterSensorNum, RightCenterSensorNum, RightSensorNum)[0];
		acc = AI(LeftSensorNum, LeftCenterSensorNum, RightCenterSensorNum, RightSensorNum)[1];
		if (acc == 0 && speed != 0)
			acc = -1;

		//Поворачиваем машину
		direction = Mathf.Sign(Vector2.Dot(RigidBody.velocity, RigidBody.GetRelativeVector(Vector2.up)));
		RigidBody.rotation += steeringAmount * steeringPower * speed * direction;
		//Debug.Log(RigidBody.rotation);
		//Debug.Log(RigidBody.position);

		//Едем только вперед
		if (speed < 0)
			speed = 0;

		var finish = GameObject.FindGameObjectsWithTag("finish");
		finishPos = finish[0].transform.position;
		Debug.Log(finishPos);


		//Увеличиваем скорость
		if (maxSpeed >= speed)
			speed = speed + acc * accelerationPower * 5 * Time.deltaTime;
		else
			speed = maxSpeed;

		Vector2 v = transform.up;

		//Передвигаем машину
		RigidBody.position += speed * v * Time.deltaTime;

		//Левый 
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

		//Левый передний
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

		//Правый передний
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

	private void OnColliderEnter2D(Collider2D collision)
	{
		if (collision.tag == "finish")
		{
			gameRestart();
		}
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.tag == "finish")
		{
			gameRestart();
		}
	}
}
