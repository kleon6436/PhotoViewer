/**
 * @file	ImageControlLibrary.h
 * @author	kchary6436
 */

#ifndef IMAGECONTROLLIBRARY_H_
#define IMAGECONTROLLIBRARY_H_

#ifdef __cplusplus
#define DllExport extern "C" __declspec(dllexport)
#else
#define DllExport __declspec(dllexport)
#endif

#include <iostream>
#include "ImageData.h"

namespace Kchary::ImageController::Library
{
	/**
	 * @brief	This function is getting raw image data using libraw library.
	 *
	 * @param	const char* path: Raw image file path.
	 * @param	ImageData* imageData: Image data
	 *
	 * @return	Success: 0, Failure: -1
	 */
	DllExport int GetRawImageData(const char* path, ImageData* imageData);

	/**
	 * @brief	This function is getting raw thumbnail image data using libraw library.
	 *
	 * @param	const char* path: Raw image file path.
	 * @param	int resizeLongSideLength: Long side length of a resize image.
	 * @param	ImageData* imageData: Image data
	 *
	 * @return	Success: 0, Failure: -1
	 */
	DllExport int GetRawThumbnailImageData(const char* path, int resizeLongSideLength, ImageData* imageData);

	/**
	 * @brief	This function is getting image data.
	 * 
	 * @param	const char* path: Raw image file path.
	 * @param	ImageData* imageData: Image data
	 *
	 * @return	Success: 0, Failure: -1
	 */
	DllExport int GetNormalImageData(const char* path, ImageData* imageData);

	/**
	 * @brief	This function is getting thumbnail image data.
	 *
	 * @param	const char* path: Raw image file path.
	 * @param	int resizeLongSideLength: Long side length of a resize image.
	 * @param	ImageData* imageData: Image data
	 *
	 * @return	Success: 0, Failure: -1
	 */
	DllExport int GetNormalThumbnailImageData(const char* path, int resizeLongSideLength, ImageData* imageData);

	/**
	 * @brief	Release the memory acquired on the DLL side.
	 *
	 * @param	uint8_t* buffer: Memory pointer you want to release.
	 */
	DllExport void FreeBuffer(std::uint8_t* buffer);
}

#endif // IMAGECONTROLLIBRARY_H_