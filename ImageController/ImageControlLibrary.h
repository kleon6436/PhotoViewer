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
	 * @param	const wchar_t* wpath: Raw image file path.
	 * @param	ImageData* imageData: Image data
	 *
	 * @return	Success: 0, Failure: -1
	 */
	DllExport int GetRawImageData(const wchar_t* wpath, ImageData* imageData);

	/**
	 * @brief	This function is getting raw thumbnail image data using libraw library.
	 *
	 * @param	const wchar_t* wpath: Raw image file path.
	 * @param	int resizeLongSideLength: Long side length of a resize image.
	 * @param	ImageData* imageData: Image data
	 *
	 * @return	Success: 0, Failure: -1
	 */
	DllExport int GetRawThumbnailImageData(const wchar_t* wpath, int resizeLongSideLength, ImageData* imageData);

	/**
	 * @brief	This function is getting image data.
	 *
	 * @param	const wchar_t* wpath: Raw image file path.
	 * @param	ImageData* imageData: Image data
	 *
	 * @return	Success: 0, Failure: -1
	 */
	DllExport int GetNormalImageData(const wchar_t* wpath, ImageData* imageData);

	/**
	 * @brief	This function is getting thumbnail image data.
	 *
	 * @param	const wchar_t* wpath: Raw image file path.
	 * @param	int resizeLongSideLength: Long side length of a resize image.
	 * @param	ImageData* imageData: Image data
	 *
	 * @return	Success: 0, Failure: -1
	 */
	DllExport int GetNormalThumbnailImageData(const wchar_t* wpath, int resizeLongSideLength, ImageData* imageData);

	/**
	 * @brief	Release the memory acquired on the DLL side.
	 *
	 * @param	uint8_t* buffer: Memory pointer you want to release.
	 */
	DllExport void FreeBuffer(std::uint8_t* buffer);

	/**
	 * @brief	Convert wchar to char.
	 *
	 * @param	const wchar_t* wpath: Raw image file path. (wchar)
	 * @param	const size_t requestBufferSize: char array buffer size.
	 * @param	char* path: Raw image file path. (char)
	 *
	 * @return	Success: 0, Failure: not 0
	 */
	int ConvertWcharToChar(const wchar_t* wpath, const size_t requestBufferSize, char* path);
}

#endif // IMAGECONTROLLIBRARY_H_