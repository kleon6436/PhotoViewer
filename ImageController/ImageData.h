/**
 * @file	ImageData.h
 * @author	kchary6436
 */

#ifndef IMAGEDATA_H_
#define IMAGEDATA_H_

typedef struct ImageData
{
	std::uint8_t* buffer;
	unsigned int size;
	int stride;
	int width;
	int height;
} ImageData;

#endif // IMAGEDATA_H_