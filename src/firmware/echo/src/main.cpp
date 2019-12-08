#include "Arduino.h"

constexpr size_t bufLen = 1024 * 30;
char* buffer = new char[bufLen];

int calcCheckSum(size_t strlen);
int32_t readString();
void panic();
void yield() {}

void setup()
{
    pinMode(LED_BUILTIN, OUTPUT);
    Serial1.begin(230400);
}

void loop()
{
    int32_t strLen = readString();
    if (strLen != 1024 * 25) panic(); // we send 25kB blocks

    int chkSum = calcCheckSum(strLen);
    if (chkSum != 2400596) panic();
}

int32_t readString()
{
    char* p = buffer;
    char* bufEnd = buffer + bufLen;

    while (true)
    {
        size_t av = Serial.available();
        while (av > 0 && p + av < bufEnd)
        {
            p += Serial.readBytes(p, av);
            if (*(p - 1) == 0)
            {
                return p - buffer;
            }
            av = Serial.available();
        }
        if (av > 0) return -1; // buffer overrun

        // blink while waiting for data
        static elapsedMillis stopwatch = 0;
        if (stopwatch > 250)
        {
            stopwatch = 0;
            digitalWriteFast(LED_BUILTIN, !digitalReadFast(LED_BUILTIN));
        }
    }
}

int calcCheckSum(size_t strLen)
{
    int chkSum = 0;
    for (size_t i = 0; i < strLen; i++)
    {
        chkSum += buffer[i];
    }
    return chkSum;
}

void panic()
{
    while (1)
    {
        digitalWriteFast(LED_BUILTIN, !digitalReadFast(LED_BUILTIN));
        delay(25);
    }
}
