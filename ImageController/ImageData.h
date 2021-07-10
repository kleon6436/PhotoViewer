/**
 * @file	ImageData.h
 * @author	kleon6436
 */

#pragma once

typedef struct ImageData
{
	std::uint8_t* buffer;
	unsigned int size;
	int stride;
	int width;
	int height;
} ImageData;