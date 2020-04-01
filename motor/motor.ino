#include <String.h>
#include <string.h>


int Led=13; //led接13
int buttonpin=3; //定義光遮斷器接3
int val; //
//int a=65;
boolean b=true;
/* rcv_text[9]="R1000000";//1+7位 
//char send_text1[9]="Go right";//8位 
//char send_text2[8];//7位 
//char send_text3[6]="steps";//9位 //rcv_text去掉字首
//char send_text_sum[15];//9位*/
String rcv_text = "N0000000"; 
String steps_text = "1000000";
int steps = 1000000;
//String send_text = "Go right 1000000 steps"; 
char a;//收到的個別字元
int c=0;

void setup()
    {
    Serial.begin(9600);
    Serial.println("Arduino Test"); 
    pinMode(Led,OUTPUT);//定義LED為輸出接口
    pinMode(buttonpin,INPUT); //定義光遮斷器輸出接口
    }
void loop(){
   c=0;
   while (Serial.available()) {
     if (Serial.available() > 0) {
    a=Serial.read();
   // Serial.write(a);
    rcv_text[c] = a;
    c=c+1;
    }
    
    
    if(c==5){
      //Serial.print(rcv_text);
    }
   // Serial.println(c);
     delay(5);  



  // delay(50);  
  } 
  delay(50);
   //c=0;
//-----獨立出數字的部分-------------

  for(int i=0;i<7;i++){
      steps_text[i] = rcv_text[i+1];
 }
  steps = steps_text.toInt();


  delay(50);
  //---------回送資料段-----------
  if(rcv_text[0]=='L'){//向左
   String send_text = String("Go left ");
   send_text = String(send_text+steps_text);
   
  Serial.println(send_text); 

  
  }else if(rcv_text[0]=='R'){//向右
   String send_text = String("Go right ");
   send_text = String(send_text+steps_text);
   Serial.println(send_text); 

   
  }else if(rcv_text[0]=='S'){//急停 S0000000
    
    Serial.println("stop!"); 
  }else{
    //Serial.println("Invalid command"); 
    
  }










   rcv_text="N0000000";
  
  
  /*
  //char rcv = Serial.read();
  //Serial.println(a);             //輸出 65 (預設 DEC)
  /*if(recieve == "a")
  {
    Serial.println("what the fuck"); 
    }
  if (Serial.available()) {

        rcv_text = Serial.read();
        
        if(rcv_text[0]=='L'){
          steps_text = "1000000";
            

          
          String send_text = String("Go left ");
          send_text = String(send_text+steps_text);
          
          Serial.println(rcv_text);          
          Serial.println(send_text);
        }else if(rcv_text[0]=='R')
        {
          
          for(int i=0;i<7;i++){
            
            steps_text[i] = rcv_text[i+1];
            }
            steps = steps_text.toInt();
          

          
          String send_text = String("Go right ");
          send_text = String(send_text+steps_text);
          
                    
          Serial.println(send_text);
          }
        
        delay(5);    // 沒有延遲的話 UART 串口速度會跟不上Arduino的速度，會導致資料不完整
    }
  //Serial.println(a,DEC);    //輸出 65
  //Serial.println(a,BIN);     //輸出 1000001
//  Serial.println(a,HEX);    //輸出 41
 // Serial.println(a,OCT);    //輸出 101
 // Serial.println(b);             //輸出 1
  //Serial.println(false);        //輸出 0
  delay(100);
    
    
    
    
    
    
    
    val=digitalRead(buttonpin); //將接口3的值讀取存至val
    if(val ==HIGH) //給條件
      {
      digitalWrite(Led,HIGH);
      }
      else
      {
      digitalWrite(Led,LOW);
      }*/
    }
