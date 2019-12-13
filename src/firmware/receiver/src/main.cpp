#include "Arduino.h"

constexpr int dataSize = 1024 * 25;     // We expect 25kB blocks
constexpr size_t bufLen = 2 * dataSize; // just to be sure
char buffer[bufLen];

elapsedMillis stopwatch = 0; // for blinking

// simple check of received data -------------------------------
void panic()
{
    while (true)
    {
        digitalWriteFast(LED_BUILTIN, !digitalReadFast(LED_BUILTIN));
        delay(25);
    }
}

void checkBuf() // data is random bytes, last byte 0, first byte checksum
{
    if (buffer[dataSize - 1] != 0)
    {
        Serial.printf("%d Wrong terminator\n", millis());
        Serial1.println("Wrong terminator");        
        return;
    }

    uint8_t chkSum = 0;
    for (size_t i = 1; i < dataSize; i++)
    {
        chkSum += buffer[i];
    }

    if (chkSum != buffer[0])
    {
        Serial.printf("%d ChecksumError   \n", millis());
        Serial1.println("Checksum Error");
    }
}

//=======================================================================

void setup()
{
    pinMode(LED_BUILTIN, OUTPUT);

    // debug info on Serial1
    Serial1.begin(230400);
    Serial1.println("Start");

    Serial.setTimeout(100);
    Serial.clear();
}

void loop()
{
    if (Serial.available() > 0)
    {
        int cnt = Serial.readBytes(buffer, dataSize); // Read in one 25kB block
        if (cnt != dataSize)
        {
            Serial1.printf("wrong size: %d\n", cnt);
            Serial.printf("%d Wrong size (%d)     \n", millis(), cnt);
        }
        else   checkBuf();             // check for correct terminator and checksum
        delayMicroseconds(500); // Remove to run stable
    }

    // heartbeat
    if (stopwatch > 250)
    {
        stopwatch = 0;
        digitalWriteFast(LED_BUILTIN, !digitalReadFast(LED_BUILTIN));
    }
}
