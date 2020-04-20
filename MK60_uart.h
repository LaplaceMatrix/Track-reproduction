/*!
 *     COPYRIGHT NOTICE
 *     Copyright (c) 2013,ɽ��Ƽ�
 *     All rights reserved.
 *     �������ۣ�ɽ����̳ http://www.vcan123.com
 *
 *     ��ע�������⣬�����������ݰ�Ȩ����ɽ��Ƽ����У�δ����������������ҵ��;��
 *     �޸�����ʱ���뱣��ɽ��Ƽ��İ�Ȩ������
 *
 * @file       MK60_uart.h
 * @brief      uart����ͷ�ļ�
 * @author     ɽ��Ƽ�
 * @version    v5.2
 * @date       2014-10-09
 */

#ifndef __MK60_UART_H__
#define __MK60_UART_H__

typedef enum
{
    UART0,
    UART1,
    UART2,
    UART3,
    UART4,
    UART5,

    UART_MAX,
} UARTn_e;

extern UART_MemMapPtr UARTN[UART_MAX];
/********************************************************************/


//��ʼ��
extern void   uart_init      (UARTn_e, uint32 baud);             //��ʼ��uartxģ��

//������ش���
extern char uart_getchar (UARTn_e uratn);                //�ȴ�����1���ֽ�

extern char   uart_querychar (UARTn_e, char *ch);                //��ѯ����1���ַ�
extern uint32 uart_querystr  (UARTn_e, char *str, uint32 max_len); //��ѯ�����ַ���
extern uint32 uart_querybuff (UARTn_e, char *buff,uint32 max_len); //��ѯ����buff

extern char   uart_query     (UARTn_e);                          //��ѯ�Ƿ���յ�һ���ֽ�


//������ش���
extern void uart_putchar   (UARTn_e, char ch);                 //����1���ֽ�
extern void uart_putbuff   (UARTn_e , uint8 *buff, uint32 len);//����len���ֽ�buff
extern void uart_putstr    (UARTn_e , const uint8 *str);       //�����ַ���



//�ж���ش���
extern void uart_rx_irq_en (UARTn_e);                          //�����ڽ����ж�
extern void uart_tx_irq_en (UARTn_e);                          //�����ڷ����ж�
extern void uart_txc_irq_en (UARTn_e);                         //�����ڷ�������ж�

extern void uart_rx_irq_dis(UARTn_e);                          //�ش��ڽ����ж�
extern void uart_tx_irq_dis(UARTn_e);                          //�ش��ڷ����ж�
extern void uart_txc_irq_dis(UARTn_e);                         //�ش��ڷ�������ж�

extern void uart3_test_handler(void);                        //�жϷ������������ο������û�����ʵ�֣�




#define FreeCarsIRQ      IRQ_UART0
#define FreeCarsBR       115200       //������

//FreeCars��λ�� ����������ʾ�� ����ͨ������������λ�����øı�
#define UartDataNum      17	    
/***************************Rocket*****************************************/
void sendDataToRocket(void);
void Rocketpush(uint8 ch,uint16 data) ;
/***************************Rocket*****************************************/


//���²�Ҫ�޸�
#define UartRxBufferLen  100
#define UartRxDataLen    41           //FreeCars��λ�����͸�������MCU���գ���Ҫ��
#define UartRxCmdLen     7	      //FreeCars��λ�������������ݳ��ȣ���Ҫ��

#define UartCmdNum  SerialPortRx.Buffer[SerialPortRx.Stack-3]//�����
#define UartCmdData SerialPortRx.Buffer[SerialPortRx.Stack-2]//��������
//�������ݵĳ���ֻҪ�����鳤��Ϊ26=22+3+1������Ϊ���뷢���ַ���ȡ��ͳһ
//ȡ���ݵĳ������ַ����ĳ�����ȣ������ڷ��������ǻ�෢����һЩ
//��Чλ������Ӱ�첻���
//fuzhi
typedef struct 
{
  int Stack;
  uint8 Data;
  uint8 PreData;
  uint8 Buffer[UartRxBufferLen];
  uint8 Enable;
  uint8 Check;
}SerialPortType;

extern uint8 uSendBuf[UartDataNum*2];
extern SerialPortType SerialPortRx;
extern double UartData[9];
uint8 UartGetOneByte(UART_MemMapPtr uartch);

void UartDebug(void);

void push(uint8 chanel,uint16 data)    ;
 void sendDataToScope(void)          ;
















#endif /* __MK60_UART_H__ */
