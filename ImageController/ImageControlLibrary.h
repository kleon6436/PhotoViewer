/**
 * @file	ImageControlLibrary.h
 * @author	kleon6436
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
	 * @brief	LibRawライブラリを用いてRaw画像データを取得する
	 * @param	imagePath: 画像ファイルパス
	 * @param	imageData: 画像データ
	 * @return	成功: 0, 失敗: -1
	 */
	DllExport int GetRawImageData(const wchar_t imagePath[], ImageData& imageData);

	/**
	 * @brief	LibRawライブラリを用いてRaw画像のサムネイルデータを取得する
	 * @param	imagePath: 画像ファイルパス
	 * @param	resizeLongSideLength: リサイズする長辺の長さ
	 * @param	imageData: 画像データ
	 * @return	成功: 0, 失敗: -1
	 */
	DllExport int GetRawThumbnailImageData(const wchar_t imagePath[], int resizeLongSideLength, ImageData& imageData);

	/**
	 * @brief	画像データを取得する(Raw画像以外)
	 * @param	imagePath: 画像ファイルパス
	 * @param	imageData: 画像データ
	 * @return	成功: 0, 失敗: -1
	 */
	DllExport int GetNormalImageData(const wchar_t imagePath[], ImageData& imageData);

	/**
	 * @brief	画像のサムネイルデータを取得する(Raw画像以外)
	 * @param	imagePath: 画像ファイルパス
	 * @param	resizeLongSideLength: リサイズする長辺の長さ
	 * @param	imageData: 画像データ
	 * @return	成功: 0, 失敗: -1
	 */
	DllExport int GetNormalThumbnailImageData(const wchar_t imagePath[], int resizeLongSideLength, ImageData& imageData);

	/**
	 * @brief	メモリ解放(DLL側で確保したメモリ)
	 * @param	buffer: メモリ配列へのポインタ
	 */
	DllExport void FreeBuffer(const std::uint8_t* buffer);

	/**
	 * @brief	wcharをchar配列に変換する
	 * @param	imagePath: wchar配列の画像ファイルパス
	 * @return	char配列のユニークポインタ(画像ファイルパス)
	 */
	std::unique_ptr<char[]> ConvertWcharToChar(const wchar_t imagePath[]);
}