/**
 * @file	ImageControlLibrary.h
 * @author	kchary6436
 */

# pragma once

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
	 * @param	imagePath: Raw image file path.
	 * @param	imageData: Image data.
	 * @return	Success: 0, Failure: -1
	 */
	DllExport int GetRawImageData(const wchar_t* imagePath, ImageData* imageData);

	/**
	 * @brief	This function is getting raw thumbnail image data using libraw library.
	 * @param	imagePath: Raw image file path.
	 * @param	resizeLongSideLength: Long side length of a resize image.
	 * @param	imageData: Image data.
	 * @return	Success: 0, Failure: -1
	 */
	DllExport int GetRawThumbnailImageData(const wchar_t* imagePath, int resizeLongSideLength, ImageData* imageData);

	/**
	 * @brief	This function is getting image data.
	 * @param	imagePath: Raw image file path.
	 * @param	imageData: Image data.
	 * @return	Success: 0, Failure: -1
	 */
	DllExport int GetNormalImageData(const wchar_t* imagePath, ImageData* imageData);

	/**
	 * @brief	This function is getting thumbnail image data.
	 * @param	imagePath: Raw image file path.
	 * @param	resizeLongSideLength: Long side length of a resize image.
	 * @param	imageData: Image data.
	 * @return	Success: 0, Failure: -1
	 */
	DllExport int GetNormalThumbnailImageData(const wchar_t* imagePath, int resizeLongSideLength, ImageData* imageData);

	/**
	 * @brief	Release the memory acquired on the DLL side.
	 * @param	buffer: Memory pointer you want to release.
	 */
	DllExport void FreeBuffer(const std::uint8_t* buffer);

	/**
	 * @brief	Convert wchar to char.
	 * @param	imagePath: Raw image file path. (wchar)
	 * @return	char array unique ptr: Converted image path.
	 */
	std::unique_ptr<char[]> ConvertWcharToChar(const wchar_t* imagePath);
}